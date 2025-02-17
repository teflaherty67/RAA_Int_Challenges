
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
    }
}
