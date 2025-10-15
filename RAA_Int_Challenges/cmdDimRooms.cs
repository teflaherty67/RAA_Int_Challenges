using RAA_Int_Challenges.Common;

namespace RAA_Int_Challenges
{
    [Transaction(TransactionMode.Manual)]
    public class cmdDimRooms : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            // create a counter for the number of dimensions created
            int dimCount = 0;

            // get all rooms in the view
           FilteredElementCollector colRooms = new FilteredElementCollector(curDoc, curDoc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType();

            // create & start a transaction
            using (Transaction t = new Transaction(curDoc))
            {
                t.Start("Dimension rooms");

                // loop through the rooms
                foreach (SpatialElement curRoom in colRooms)
                {
                    // set boundary options
                    SpatialElementBoundaryOptions sebOptions = new SpatialElementBoundaryOptions();
                    sebOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish;

                    // create empty reference arrays & point lists
                    ReferenceArray refArrayVer = new ReferenceArray();
                    ReferenceArray refArrayHor = new ReferenceArray();

                    List<XYZ> listPointsVer = new List<XYZ>();
                    List<XYZ> listPointsHor = new List<XYZ>();

                    // get room boundaries
                    List<BoundarySegment> boundarySegments = curRoom.GetBoundarySegments(sebOptions).First().ToList();

                    // loop through the boundary segments
                    foreach(BoundarySegment curSeg in boundarySegments)
                    {
                        // get boundary geometry & midpoint
                        Curve curCurve = curSeg.GetCurve();
                        XYZ midPoint = curCurve.Evaluate(0.5, true);

                        // get boundary wall
                        Element curWall = curDoc.GetElement(curSeg.ElementId);

                        // null check
                        if (curWall != null)
                        {
                            // check if wall (line) is vertical
                            if (Utils.IsLineVertical(curCurve))
                            {
                                // add to vertical reference array
                                refArrayVer.Append(new Reference(curWall));

                                // add to vertical point list
                                listPointsVer.Add(midPoint);
                            }
                            else
                            {
                                // add to horizontal reference array
                                refArrayHor.Append(new Reference(curWall));

                                // add to horizontal point list
                                listPointsHor.Add(midPoint);
                            }
                        }
                    }

                    // create lines for dimensions
                    XYZ offsetVer = new XYZ(3, 0, 0);
                    XYZ offsetHor = new XYZ(0, 3, 0);

                    XYZ p1V = listPointsVer.First().Add(offsetVer);
                    XYZ p2V = new XYZ(listPointsVer.Last().X, listPointsVer.First().Y,0).Add(offsetVer);

                    XYZ p1H = listPointsHor.First().Add(offsetHor);
                    XYZ p2H = new XYZ(listPointsHor.First().X, listPointsHor.Last().Y, 0).Add(offsetHor);

                    Line dimLineVer = Line.CreateBound(p1V, p2V);
                    Line dimLineHor = Line.CreateBound(p1H.Add(offsetHor), p2H.Add(offsetHor));

                    // create dimensions
                    Dimension newDimVer = curDoc.Create.NewDimension(curDoc.ActiveView, dimLineVer, refArrayVer);
                    Dimension newDimHor = curDoc.Create.NewDimension(curDoc.ActiveView, dimLineHor, refArrayHor);

                    dimCount += 2;
                }

                // commit the changes
                t.Commit();
            }

            // report to the user
            TaskDialog.Show("Complete", $"Created {dimCount} dimension strings.");

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
