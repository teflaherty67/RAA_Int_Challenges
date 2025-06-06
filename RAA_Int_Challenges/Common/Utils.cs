﻿
using Autodesk.Revit.DB.Architecture;
using System.Windows.Controls;

namespace RAA_Int_Challenges.Common
{
    internal static class Utils
    {
        internal static RibbonPanel CreateRibbonPanel(UIControlledApplication app, string tabName, string panelName)
        {
            RibbonPanel curPanel;

            if (GetRibbonPanelByName(app, tabName, panelName) == null)
                curPanel = app.CreateRibbonPanel(tabName, panelName);

            else
                curPanel = GetRibbonPanelByName(app, tabName, panelName);

            return curPanel;
        }      

        internal static RibbonPanel GetRibbonPanelByName(UIControlledApplication app, string tabName, string panelName)
        {
            foreach (RibbonPanel tmpPanel in app.GetRibbonPanels(tabName))
            {
                if (tmpPanel.Name == panelName)
                    return tmpPanel;
            }

            return null;
        }

        internal static ViewSchedule CreateRoomScheduleByDepartment(Document curDoc, string curDept)
        {
            // 4a. create schedule
            ViewSchedule curSched = CreateSchedule(curDoc, BuiltInCategory.OST_Rooms, $"Department - {curDept}");

            // 4b. get room instance
            Element roomInst = GetAllRooms(curDoc).First();

            // 4c. Add fields to schedule
            ScheduleField roomNumField = AddFieldToSchedule(curSched, ScheduleFieldType.Instance, GetParameterByName(roomInst, "Number"), false);
            ScheduleField roomNameField = AddFieldToSchedule(curSched, ScheduleFieldType.Instance, GetParameterByName(roomInst, "Name"), false);
            ScheduleField deptField = AddFieldToSchedule(curSched, ScheduleFieldType.Instance, GetParameterByName(roomInst, "Department"), false);
            ScheduleField commentsField = AddFieldToSchedule(curSched, ScheduleFieldType.Instance, GetParameterByName(roomInst, "Comments"), false);
            ScheduleField areaField = AddFieldToSchedule(curSched, ScheduleFieldType.ViewBased, GetParameterByName(roomInst, BuiltInParameter.ROOM_AREA), false);
            ScheduleField levelField = AddFieldToSchedule(curSched, ScheduleFieldType.Instance, GetParameterByName(roomInst, "Level"), true);

            areaField.DisplayType = ScheduleFieldDisplayType.Totals;

            // 4d. filter schedule by department
            ScheduleFilter deptFilter = new ScheduleFilter(deptField.FieldId, ScheduleFilterType.Equal, curDept);
            curSched.Definition.AddFilter(deptFilter);

            // 4e. sort and group schedule
            ScheduleSortGroupField sortLevel = new ScheduleSortGroupField(levelField.FieldId, ScheduleSortOrder.Ascending);
            sortLevel.ShowHeader = true;
            sortLevel.ShowFooter = true;
            sortLevel.ShowBlankLine = true;
            curSched.Definition.AddSortGroupField(sortLevel);

            ScheduleSortGroupField sortRoomName = new ScheduleSortGroupField(roomNameField.FieldId, ScheduleSortOrder.Ascending);
            curSched.Definition.AddSortGroupField(sortRoomName);

            // 4f. set grand total properties
            curSched.Definition.IsItemized = true;
            curSched.Definition.ShowGrandTotal = true;
            curSched.Definition.ShowGrandTotalTitle = true;
            curSched.Definition.ShowGrandTotalCount = true;

            return curSched;
        }

        private static ScheduleField AddFieldToSchedule(ViewSchedule curSched, ScheduleFieldType fieldType, Parameter parameter, bool isHidden)
        {
            ScheduleField curField = curSched.Definition.AddField(fieldType, parameter.Id);
            curField.IsHidden = isHidden;

            return curField;
        }

        private static Parameter GetParameterByName(Element curElem, string paramName)
        {
            Parameter curParam = curElem.LookupParameter(paramName);

            return curParam;
        }

        private static Parameter GetParameterByName(Element curElem, BuiltInParameter bip)
        {
            Parameter curParam = curElem.get_Parameter(bip);

            return curParam;
        }

        internal static ViewSchedule CreateSchedule(Document curDoc, BuiltInCategory bic, string name)
        {
            ElementId catId = new ElementId(bic);

            ViewSchedule newSchedule = ViewSchedule.CreateSchedule(curDoc, catId);
            newSchedule.Name = name;

            return newSchedule;
        }

        internal static List<Element> GetAllRooms(Document curDoc)
        {
            FilteredElementCollector m_colRooms = new FilteredElementCollector(curDoc)
                .OfCategory(BuiltInCategory.OST_Rooms);

            return m_colRooms.ToList();
        }

        internal static List<string> GetAllDepartmentsByName(Document curDoc, List<Element> listRooms)
        {
            List<string> m_rawDepts = new List<string>();

            foreach (Room curRoom in listRooms)
            {
                string nameDept = curRoom.LookupParameter("Department").AsString();
                m_rawDepts.Add(nameDept);
            }

            List<string> m_uniqueDepts = m_rawDepts.Distinct().ToList();
            m_uniqueDepts.Sort();

            return m_uniqueDepts;
        }

        internal static ViewSchedule CreateDepartmentSchedule(Document curDoc)
        {
            // 5a. Create schedule
            ViewSchedule curSched = CreateSchedule(curDoc, BuiltInCategory.OST_Rooms, "All Departments");

            // 5b. Get instance of room
            Element room = GetAllRooms(curDoc).First();

            // 5c. Add fields to schedule
            ScheduleField deptField = AddFieldToSchedule(curSched, ScheduleFieldType.Instance, GetParameterByName(room, "Department"), false);
            ScheduleField areaField = AddFieldToSchedule(curSched, ScheduleFieldType.ViewBased, GetParameterByName(room, BuiltInParameter.ROOM_AREA), false);

            areaField.DisplayType = ScheduleFieldDisplayType.Totals;

            // 5d. Sort schedule by department
            ScheduleSortGroupField deptSort = new ScheduleSortGroupField(deptField.FieldId, ScheduleSortOrder.Ascending);
            curSched.Definition.AddSortGroupField(deptSort);

            curSched.Definition.IsItemized = false;
            curSched.Definition.ShowGrandTotal = true;
            curSched.Definition.ShowGrandTotalTitle = true;

            return curSched;
        }

        internal static Dictionary<ViewType, List<BuiltInCategory>> GetViewTypeCatDictionary()
        {
            // create an empty dictionary to hold the keys & values
            Dictionary<ViewType, List<BuiltInCategory>> m_dictionary = new Dictionary<ViewType, List<BuiltInCategory>>();

            // add the keys and values to the dictionary
            m_dictionary.Add(ViewType.FloorPlan, new List<BuiltInCategory>
            {
                BuiltInCategory.OST_Rooms,
                BuiltInCategory.OST_Windows,
                BuiltInCategory.OST_Doors,
                BuiltInCategory.OST_Furniture,
                BuiltInCategory.OST_Walls
            });
            m_dictionary.Add(ViewType.AreaPlan, new List<BuiltInCategory> { BuiltInCategory.OST_Areas });

            m_dictionary.Add(ViewType.CeilingPlan, new List<BuiltInCategory>
            {
                BuiltInCategory.OST_Rooms,
                BuiltInCategory.OST_LightingFixtures
            });

            m_dictionary.Add(ViewType.Section, new List<BuiltInCategory> { BuiltInCategory.OST_Rooms });

            return m_dictionary;
        }

        internal static XYZ GetInsertPoint(Location curLoc)
        {
            LocationPoint locPoint = curLoc as LocationPoint;
            XYZ point;

            if (locPoint != null)
            {
                point = locPoint.Point;
            }
            else
            {
                LocationCurve locCurve = curLoc as LocationCurve;
                point = MidpointBetweenTwoPoints(locCurve.Curve.GetEndPoint(0), locCurve.Curve.GetEndPoint(1));
            }

            return point;
        }

        private static XYZ MidpointBetweenTwoPoints(XYZ point1, XYZ point2)
        {
            XYZ midPoint = new XYZ((point1.X + point2.X) / 2, (point1.Y + point2.Y) / 2, (point1.Z + point2.Z) / 2);
            
            return midPoint;
        }

        internal static Dictionary<string, FamilySymbol> GetTagDictionary(Document curDoc)
        {
            // create empty dictionary
            Dictionary<string, FamilySymbol> m_tagDict = new Dictionary<string, FamilySymbol>();

            // add keys & values to empty dictionary
            m_tagDict.Add("Rooms", GetTagByName(curDoc, "M_Room Tag"));
            m_tagDict.Add("Doors", GetTagByName(curDoc, "M_Door Tag"));
            m_tagDict.Add("Windows", GetTagByName(curDoc, "M_Window Tag"));
            m_tagDict.Add("Furniture", GetTagByName(curDoc, "M_Furniture Tag"));
            m_tagDict.Add("Lighting Fixtures", GetTagByName(curDoc, "M_Lighting Fixture Tag"));
            m_tagDict.Add("Walls", GetTagByName(curDoc, "M_Wall Tag"));
            m_tagDict.Add("Curtain Walls", GetTagByName(curDoc, "M_Curtain Wall Tag"));
            m_tagDict.Add("Areas", GetTagByName(curDoc, "M_Area Tag"));

            return m_tagDict;
        }

        private static FamilySymbol GetTagByName(Document curDoc, string tagName)
        {
            return new FilteredElementCollector(curDoc)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .Where(x => x.FamilyName.Equals(tagName))
                .First();

        }

        internal static bool IsCurtainWall(Element curElem)
        {
            Wall curWall = curElem as Wall;

            if (curWall.WallType.Kind == WallKind.Curtain)
                return true;

            return false;
        }

        internal static List<View> GetAllViews(Document curDoc)
        {
            // gets all views and excludes any view templates
            List<View> m_returnList = new List<View>();

            FilteredElementCollector m_colViews = new FilteredElementCollector(curDoc)
                .OfCategory(BuiltInCategory.OST_Views)
                .WhereElementIsNotElementType();

            foreach (View view in m_colViews)
            {
                if (view.IsTemplate == false)
                    m_returnList.Add(view);
            }

            return m_returnList;
        }
    }
}
