using RAA_Int_Challenges.Common;

namespace RAA_Int_Challenges
{
    [Transaction(TransactionMode.Manual)]
    public class cmdDimGrid : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            // get all grid lines
            FilteredElementCollector colGrids = new FilteredElementCollector(curDoc, curDoc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_Grids)
                .WhereElementIsNotElementType();

            // create empty reference arrays & point lists
            ReferenceArray refArrayVer = new ReferenceArray();
            ReferenceArray refArrayHor = new ReferenceArray();

            List<XYZ> listPointsVer = new List<XYZ>();
            List<XYZ> listPointsHor = new List<XYZ>();

            // loop through the grids & check if vertical or horizontal
            foreach (Grid curGrid in colGrids)
            {
                // cast each grid to a line
                Line gridLine = curGrid.Curve as Line;

                // check if vertical
                if (Utils.IsLineVertical(gridLine))
                {
                    // add to vertical reference array
                    refArrayVer.Append(new Reference(curGrid));

                    // get the endpoint & add to point list
                    XYZ point1 = gridLine.GetEndPoint(1);
                    listPointsVer.Add(point1);
                }
                else
                {
                    // add to horizontal reference array
                    refArrayHor.Append(new Reference(curGrid));

                    // get the endpoint & add to point list
                    XYZ point1 = gridLine.GetEndPoint(1);
                    listPointsHor.Add(point1);
                }
            }

            // order the point lists
            List<XYZ> orderedPointsVer = listPointsVer.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            XYZ p1V = orderedPointsVer.First();
            XYZ p2V = orderedPointsVer.Last();
            XYZ offsetVer = new XYZ(0, 3, 0);

            List<XYZ> orderedPointsHor = listPointsHor.OrderBy(p => p.Y).ThenBy(p => p.X).ToList();
            XYZ p1H = orderedPointsHor[0];
            XYZ p2H = orderedPointsHor[orderedPointsHor.Count - 1];
            XYZ offsetHor = new XYZ(-3, 0, 0);

            // create & start a transaction
            using (Transaction t = new Transaction(curDoc))
            {
                t.Start("Dimension Grids");

                // create dimension strings
                Line lineVer = Line.CreateBound(p1V.Subtract(offsetVer), p2V.Subtract(offsetVer));
                Line LINEhOR = Line.CreateBound(p1H.Subtract(offsetHor), p2H.Subtract(offsetHor));

                Dimension dimVer = curDoc.Create.NewDimension(curDoc.ActiveView, lineVer, refArrayVer);
                Dimension dimHor = curDoc.Create.NewDimension(curDoc.ActiveView, LINEhOR, refArrayHor);

                // commit the changes
                t.Commit();
            }

            return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Button 1";

            Common.clsButtonData myButtonData = new Common.clsButtonData(
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
