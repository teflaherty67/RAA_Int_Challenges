#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

#endregion

namespace RAA_Int_Challenges
{
    [Transaction(TransactionMode.Manual)]
    public class cmdSchedulePalooza : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document curDoc = uiapp.ActiveUIDocument.Document;

            // 1. get all rooms
            List<Element> listRooms = Utils.GetAllRooms(curDoc);

            // 2. get department names
            List<string> listDepts = Utils.GetAllDepartmentsByName(curDoc, listRooms);

            // create and start a transaction
            using (Transaction t = new Transaction(curDoc))
            {
                t.Start("Create Schedules");

                // 3. loop through dept list
                foreach (string curDept  in listDepts)
                {
                    // 4. create schedules
                    ViewSchedule newSchedule = Utils.CreateRoomScheduleByDepartment(curDoc, curDept);
                }
                
                // commit the changes
                t.Commit();
            }

            return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand2";
            string buttonTitle = "Button 2";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 2");

            return myButtonData1.Data;
        }
    }
}
