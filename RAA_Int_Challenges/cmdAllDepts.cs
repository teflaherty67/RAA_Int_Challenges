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
using System.Windows.Input;

#endregion

namespace RAA_Int_Challenges
{
    [Transaction(TransactionMode.Manual)]
    public class cmdAllDepts : IExternalCommand
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

            // create & start the transaction
            using (Transaction t = new Transaction(curDoc))
            {
                t.Start("Create Department Schedules");
                {
                    // get the element Id for rooms
                    ElementId catId = new ElementId(BuiltInCategory.OST_Rooms);

                    // create the schedule
                    ViewSchedule newDeptSched = ViewSchedule.CreateSchedule(curDoc, catId);
                    newDeptSched.Name = "All Departments";

                    // get parameters for schedule fields                    
                    Parameter paramRmDept = roomInst.LookupParameter("Department");
                    Parameter paramRmArea = roomInst.get_Parameter(BuiltInParameter.ROOM_AREA);

                    // create the fields                    
                    ScheduleField fieldRmDept = newDeptSched.Definition.AddField(ScheduleFieldType.Instance, paramRmDept.Id);
                    ScheduleField fieldRmArea = newDeptSched.Definition.AddField(ScheduleFieldType.ViewBased, paramRmArea.Id);

                    // create the filter
                    ScheduleFilter areaFilter = new ScheduleFilter(fieldRmArea.FieldId, ScheduleFilterType.GreaterThan, 100.0);
                    newDeptSched.Definition.AddFilter(areaFilter);

                    // set sorting & grouping
                    ScheduleSortGroupField sortRmDept = new ScheduleSortGroupField(fieldRmDept.FieldId);
                    sortRmDept.ShowHeader = true;
                    sortRmDept.ShowFooter = true;
                    sortRmDept.ShowBlankLine = true;
                    newDeptSched.Definition.AddSortGroupField(sortRmDept);

                    // set the formatting                    
                    fieldRmArea.DisplayType = ScheduleFieldDisplayType.Totals;

                    // calculate totals
                    newDeptSched.Definition.IsItemized = false;
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
