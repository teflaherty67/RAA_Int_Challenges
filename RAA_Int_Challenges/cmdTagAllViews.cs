using RAA_Int_Challenges.Common;
using System.Diagnostics.Metrics;

namespace RAA_Int_Challenges
{
    [Transaction(TransactionMode.Manual)]
    public class cmdTagAllViews : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            List<View> colViews = Utils.GetAllViews(curDoc);

            // old code that didn't filter out view templates
            //FilteredElementCollector colViews = new FilteredElementCollector(curDoc)
            //    .OfCategory(BuiltInCategory.OST_Views)
            //    .WhereElementIsNotElementType();

            // create counters for tags & views
            int countTags = 0;
            int countViews = 0;

            foreach (View curView in colViews)
            {
                // check to see if current view is view template
                if (curView.IsTemplate == true)
                    continue;

                // 01. get the active view & it's type            
                // View curView = curDoc.ActiveView;
                ViewType curViewType = curView.ViewType;

                // 02. get the categories for this view type
                List<BuiltInCategory> catList = new List<BuiltInCategory>();

                // 02a. create a dictionary to hold the categories
                Dictionary<ViewType, List<BuiltInCategory>> viewTypeCatDictionary = Utils.GetViewTypeCatDictionary();

                // 02b. try to match the view type to a dictionary
                if (viewTypeCatDictionary.TryGetValue(curViewType, out catList) == false)
                {
                    continue;
                }

                // 03. get elements to tag for the view type
                ElementMulticategoryFilter catFilter = new ElementMulticategoryFilter(catList);
                FilteredElementCollector colElem = new FilteredElementCollector(curDoc, curView.Id);
                colElem.WherePasses(catFilter).WhereElementIsNotElementType();

                // TaskDialog.Show("Test", $"Found {colElem.Count()} elements");

                // 06. create dictionary of tag family symbols
                Dictionary<string, FamilySymbol> dictionaryTags = Utils.GetTagDictionary(curDoc);

                // 04. loop through the elements and tag
                using (Transaction t = new Transaction(curDoc))
                {
                    t.Start("Tag Elements");

                    foreach (Element curElem in colElem)
                    {
                        bool addLeader = false;

                        if (curElem.Location == null)
                            continue;

                        // 05. get insertion point based on element type
                        XYZ point = Utils.GetInsertPoint(curElem.Location);

                        if (point == null)
                            continue;

                        // 07. get element category
                        string catName = curElem.Category.Name;

                        // 10. check catName for Walls
                        if (catName == "Walls")
                        {
                            addLeader = true;

                            if (Utils.IsCurtainWall(curElem))
                                catName = "Curtain Walls";
                        }

                        // 08. get tag based on element category
                        FamilySymbol elemTag = dictionaryTags[catName];

                        // 09. tag the elements
                        if (catName == "Areas")
                        {
                            ViewPlan curAreaPlan = curView as ViewPlan;
                            Area curArea = curElem as Area;

                            // create the area tag
                            AreaTag newTag = curDoc.Create.NewAreaTag(curAreaPlan, curArea, new UV(point.X, point.Y));
                            newTag.TagHeadPosition = new XYZ(point.X, point.Y, 0);
                            newTag.HasLeader = false;
                        }
                        else
                        {
                            IndependentTag newTag = IndependentTag.Create(curDoc, elemTag.Id, curView.Id,
                                new Reference(curElem), addLeader, TagOrientation.Horizontal, point);

                            // 09a. offset tags as needed
                            if (catName == "Windows")
                                newTag.TagHeadPosition = point.Add(new XYZ(0, 3, 0));

                            if (curView.ViewType == ViewType.Section)
                                newTag.TagHeadPosition = point.Add(new XYZ(0, 0, 3));
                        }

                        countTags++;
                    }

                    t.Commit();
                }
                countViews++;
            }

            TaskDialog.Show("Complete", $"Added {countTags} tags to {countViews} views.");

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
