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

        internal static List<string> GetAllDepartmentsByName(Document curDoc)
        {
            FilteredElementCollector m_colRooms = new FilteredElementCollector(curDoc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType();

            List<string> m_rawDepts = new List<string>();
            
            foreach (Room curRoom in m_colRooms)
            {
                string nameDepartment = curRoom.get_Parameter(BuiltInParameter.ROOM_DEPARTMENT).AsValueString();
                m_rawDepts.Add(nameDepartment);                
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
    }
}
