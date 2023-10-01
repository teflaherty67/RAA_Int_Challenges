#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

#endregion

namespace RAA_Int_Challenges
{
    [Transaction(TransactionMode.Manual)]
    public class cmdDeptSchedules : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document curDoc = uiapp.ActiveUIDocument.Document;

            // Get the department names
            FilteredElementCollector colRooms = new FilteredElementCollector(curDoc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType();

            Element roomInst = colRooms.FirstElement();

            List<string> nameDepartment = Utils.GetAllDepartmentsByName(curDoc, colRooms);

            // create & start the transaction
            using (Transaction t = new Transaction(curDoc))
            {
                t.Start("Create Department Schedules");

                // get the element Id for rooms
                ElementId catId = new ElementId(BuiltInCategory.OST_Rooms);

                // create a schedule for each department
                foreach (string curDept in nameDepartment)
                {
                    // create the schedule
                    ViewSchedule newDeptSched = ViewSchedule.CreateSchedule(curDoc, catId);
                    newDeptSched.Name = "Dept - " + curDept;

                    // get parameters for schedule fields
                    Parameter paramRmNum = roomInst.LookupParameter("Number");
                    Parameter paramRmLevel = roomInst.LookupParameter("Level");
                    Parameter paramRmName = roomInst.LookupParameter("Name");
                    Parameter paramRmDept = roomInst.LookupParameter("Department");
                    Parameter paramRmComments = roomInst.LookupParameter("Comments");
                    Parameter paramRmArea = roomInst.get_Parameter(BuiltInParameter.ROOM_AREA);                    

                    // create the fields
                    ScheduleField fieldRmNum = newDeptSched.Definition.AddField(ScheduleFieldType.Instance, paramRmNum.Id);
                    ScheduleField fieldRmLevel = newDeptSched.Definition.AddField(ScheduleFieldType.Instance, paramRmLevel.Id);
                    ScheduleField fieldRmName = newDeptSched.Definition.AddField(ScheduleFieldType.Instance, paramRmName.Id);
                    ScheduleField fieldRmDept = newDeptSched.Definition.AddField(ScheduleFieldType.Instance, paramRmDept.Id);
                    ScheduleField fieldRmComments = newDeptSched.Definition.AddField(ScheduleFieldType.Instance, paramRmComments.Id);
                    ScheduleField fieldRmArea = newDeptSched.Definition.AddField(ScheduleFieldType.ViewBased, paramRmArea.Id);

                    // create the filter
                    ScheduleFilter deptFilter = new ScheduleFilter(fieldRmDept.FieldId, ScheduleFilterType.Equal, curDept);
                    newDeptSched.Definition.AddFilter(deptFilter);

                    ScheduleFilter levelFilter = new ScheduleFilter(fieldRmLevel.FieldId, ScheduleFilterType.Equal, "02 - Floor");
                    newDeptSched.Definition.AddFilter(levelFilter);

                    // set sorting & grouping
                    ScheduleSortGroupField sortRmLevel = new ScheduleSortGroupField(fieldRmLevel.FieldId);
                    sortRmLevel.ShowHeader = true;
                    sortRmLevel.ShowFooter = true;
                    sortRmLevel.ShowBlankLine = true;
                    newDeptSched.Definition.AddSortGroupField(sortRmLevel);

                    ScheduleSortGroupField sortRmName = new ScheduleSortGroupField(fieldRmName.FieldId);
                    newDeptSched.Definition.AddSortGroupField(sortRmName);

                    // set the formatting
                    fieldRmLevel.IsHidden = true;
                    fieldRmArea.DisplayType = ScheduleFieldDisplayType.Totals;

                    // calculate totals
                    newDeptSched.Definition.IsItemized = true;
                    newDeptSched.Definition.ShowGrandTotal = true;
                    newDeptSched.Definition.ShowGrandTotalTitle = true;
                    newDeptSched.Definition.ShowGrandTotalCount = true;

                }

                t.Commit();
            }

                return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Button 1";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 1");

            return myButtonData1.Data;
        }
    }
}
