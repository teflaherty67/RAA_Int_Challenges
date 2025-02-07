using RAA_Int_Challenges.Common;

namespace RAA_Int_Challenges
{
    [Transaction(TransactionMode.Manual)]
    public class Command1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            // 01. get the rooms
            List<Element> listRooms = Utils.GetAllRooms(curDoc);

            // 02. get department names
            int counter = 0;
            List<string> listDepts = Utils.GetAllDepartmentsByName(curDoc, listRooms);

            // create & start a transaction
            using (Transaction t = new Transaction(curDoc))
            {
                t.Start("Create Schedules");

                // 03. loop through department list
                foreach(string curDept in listDepts)
                {
                    // .04 create the schedule
                    ViewSchedule newSchedule = Utils.CreateRoomScheduleByDepartment(curDoc, curDept);
                    counter++;
                }

                // 05. create all departments schedule
                ViewSchedule newSchedule2 = Utils.CreateDepartmentSchedule(curDoc);

                // commit the changes
                t.Commit();
            }

            // report to the user
            TaskDialog.Show("Complete", $"Created {counter} schedules.");

            return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Button 1";

            Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 1");

            return myButtonData.Data;
        }
    }

}
