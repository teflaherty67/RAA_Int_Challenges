using RAA_Int_Challenges.Common;

namespace RAA_Int_Challenges
{
    [Transaction(TransactionMode.Manual)]
    public class cmdTagView : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            // 01. get the active view & it's type            
            View curView = curDoc.ActiveView;
            ViewType curViewType = curView.ViewType;

            // 02. get the categories for this view type
            List<BuiltInCategory> catList = new List<BuiltInCategory>();

            // 02a. create a dictionary to hold the categories
            Dictionary<ViewType, List<BuiltInCategory>> viewTypeCatDictionary = Utils.GetViewTypeCatDictionary();

            // 02b. try to match the view type to a dictionary
            if(viewTypeCatDictionary.TryGetValue(curViewType, out catList) == false)
            {
                TaskDialog.Show("Error", "Cannot add tags to this view type.");
                return Result.Failed;
            }

            // 03. get elements to tag for the view type
            ElementMulticategoryFilter catFilter = new ElementMulticategoryFilter(catList);
            FilteredElementCollector colElem = new FilteredElementCollector(curDoc, curView.Id);
            colElem.WherePasses(catFilter).WhereElementIsNotElementType();

            // TaskDialog.Show("Test", $"Found {colElem.Count()} elements");

            // 06. create dictionary of tag family symbols
            Dictionary<string, FamilySymbol> dictionaryTags = Utils.GetTagDictionary(curDoc);

            // 04. loop through the elements and tag
            int counter = 0;

            foreach(Element curElem in colElem)
            {
                bool addLeader = false;

                if (curElem.Location == null)
                    continue;

                // 05. get insertion point based on element type
                XYZ point = Utils.GetInsertPoint(curElem.Location);

                if (point == null)
                    continue;

                
            }




            // 04. report to the user the number of tags placed


            return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand2";
            string buttonTitle = "Button 2";

            Common.clsButtonData myButtonData = new Common.clsButtonData(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 2");

            return myButtonData.Data;
        }
    }

}
