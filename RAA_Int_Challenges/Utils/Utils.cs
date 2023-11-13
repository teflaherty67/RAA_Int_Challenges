using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAA_Int_Challenges
{
    internal static class Utils
    {
        internal static RibbonPanel CreateRibbonPanel(UIControlledApplication app, string tabName, string panelName)
        {
            RibbonPanel currentPanel = GetRibbonPanelByName(app, tabName, panelName);

            if (currentPanel == null)
                currentPanel = app.CreateRibbonPanel(tabName, panelName);

            return currentPanel;
        }


        internal static List<string> GetAllDepartmentsByName(Document curDoc, FilteredElementCollector colRooms)
        {
            List<string> m_rawDepts = new List<string>();

            foreach (Room curRoom in colRooms)
            {
                string nameDepartment = curRoom.get_Parameter(BuiltInParameter.ROOM_DEPARTMENT).AsValueString();
                m_rawDepts.Add(nameDepartment);
            }

            List<string> m_uniqueDepts = m_rawDepts.Distinct().ToList();
            m_uniqueDepts.Sort();

            return m_uniqueDepts;
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
                string curDept = curRoom.LookupParameter("Department").AsString();
                m_rawDepts.Add(curDept);
            }

            List<string> m_uniqueDepts = m_rawDepts.Distinct().ToList();
            m_uniqueDepts.Sort();

            return m_uniqueDepts;
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
            ViewSchedule curSchedule = CreateSchedule(curDoc, BuiltInCategory.OST_Rooms, $"Department - {curDoc}");

            // 4b. get room instance
            Element roomInst = GetAllRooms(curDoc).First();

            // 4c. add fields to schedule
            ScheduleField fieldRmNum = AddFieldToSchedule(curSchedule, ScheduleFieldType.Instance, GetParameterByName(roomInst, "Number"), false);
            ScheduleField fieldRmLevel
            ScheduleField fieldRmName
            ScheduleField fieldRmDept
            ScheduleField fieldRmComments
            ScheduleField fieldRmArea   
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

        private static ViewSchedule CreateSchedule(Document curDoc, BuiltInCategory bic, string name)
        {
            ElementId catId = new ElementId(bic);

            ViewSchedule newSchedule = ViewSchedule.CreateSchedule(curDoc, catId);
            newSchedule.Name = name;

            return newSchedule;
        }
    }
}
