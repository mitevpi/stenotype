using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Stenotype
{
    /// <summary>
    /// Wrapper class for a Revit Curtain Panel Object.</summary>
    /// <remarks>
    /// ADD
    /// </remarks>
    public class CurtainPanelST
    {
        /// <summary>
        /// The document to which the Curtain Panel belongs to.
        /// </summary>
        [NonSerialized()] public readonly Document Doc;

        /// <summary>
        /// A JSON serialized string representing this class object.
        /// </summary>
        [NonSerialized()] public readonly string Serialized;

        /// <summary>
        /// The JSON object representation of this class.
        /// </summary>
        [NonSerialized()] public JObject JsonObject;

        [NonSerialized()] public readonly Panel Panel;
        [JsonProperty()] private string PanelString { get => Panel.Name; }


        /// <summary>
        /// Initialize from a Curtain Panel object.
        /// </summary>
        /// <param name="panel">The Revit Curtain Panel element.</param>
        public CurtainPanelST(Panel panel)
        {
            Doc = panel.Document;
            Panel = panel;
            Serialized = JsonConvert.SerializeObject(this);
            JsonObject = JObject.Parse(Serialized);
        }

        /// <summary>
        /// Internal method for checking approxmiate equals. Used for intersection checking as model precision is not perfect.
        /// </summary>
        /// <param name="value1">The first number.</param>
        /// <param name="value2">The second number.</param>
        /// <param name="acceptableDifference">The acceptable tolerance for returning true.</param>
        /// <returns>Bounding Box Intersection filter.</returns>
        internal static bool ApproximatelyEquals(double value1, double value2, double acceptableDifference)
        {
            return Math.Abs(value1 - value2) <= acceptableDifference;
        }

        /// <summary>
        /// Create the base bounding box intersect filter here. Any modification to the boxes or the filter should be done here.
        /// </summary>
        /// <param name="view">Revit View object.</param>
        /// <param name="scaleFactor">The scaling factor to apply to the Bounding Box.</param>
        /// <returns>Bounding Box Intersection filter.</returns>
        public BoundingBoxIntersectsFilter GetNeighborFilter(View view, double scaleFactor)
        {
            // Create bounding box and outline element for the Revit element to check
            BoundingBoxXYZ elementBoundingBox = Panel.get_BoundingBox(view);
            Outline elementOutline = new Outline(elementBoundingBox.Min, elementBoundingBox.Max);
            elementOutline.Scale(scaleFactor);
            BoundingBoxIntersectsFilter intersectsFilter = new BoundingBoxIntersectsFilter(elementOutline);

            return intersectsFilter;
        }

        /// <summary>
        /// Vanilla intersection checker using FilteredElementCollector and BoundingBoxIntersectsFilter.
        /// Does not filter vertex intersections, or other corner-to-corner conditions.
        /// </summary>
        /// <param name="view">Revit View object.</param>
        /// <returns>Collection of Panel elements.</returns>
        public ICollection<Panel> GetNeighborCurtainPanels(View view)
        {
            BoundingBoxIntersectsFilter intersectsFilter = GetNeighborFilter(view, 1.5);
            ICollection<Panel> neighborPanels = new FilteredElementCollector(Doc, view.Id).OfCategory(BuiltInCategory.OST_CurtainWallPanels).WhereElementIsNotElementType().WherePasses(intersectsFilter).Select(p => (Panel)p).ToList();

            return neighborPanels;
        }

        /// <summary>
        ///  Vanilla intersection checker + sorting by distance to the tester panel to return the X closest items.
        /// </summary>
        /// <param name="view">Revit View object.</param>
        /// <param name="sensitivity">Integer count of how many of the closest panels the function will return.</param>
        /// <returns>Collection of Panel elements.</returns>
        public IEnumerable<Panel> GetNeighborCurtainPanels(View view, int sensitivity)
        {
            BoundingBoxXYZ elementBoundingBox = Panel.get_BoundingBox(view);
            BoundingBoxIntersectsFilter intersectsFilter = GetNeighborFilter(view, 1.5);

            ICollection<Panel> neighborPanels = new FilteredElementCollector(Doc, view.Id).OfCategory(BuiltInCategory.OST_CurtainWallPanels).WhereElementIsNotElementType().WherePasses(intersectsFilter).Select(p => (Panel)p).ToList();

            Dictionary<Panel, double> distanceDict = new Dictionary<Panel, double>();
            foreach (Panel neighborPanel in neighborPanels)
            {
                BoundingBoxXYZ bb = neighborPanel.get_BoundingBox(view);
                double minDist = bb.Min.DistanceTo(elementBoundingBox.Min);
                distanceDict.Add(neighborPanel, minDist);
            }

            IEnumerable<Panel> sortedPanels = from pair in distanceDict
                                            orderby pair.Value ascending
                                            where pair.Value != 0
                                            select pair.Key;            

            return sortedPanels.Take(sensitivity);
        }

        /// <summary>
        /// Sophisticated intersection checker. Seperates neighboring panels into side conditions, above conditions, and below conditions.
        /// </summary>
        /// <param name="view">Revit View object.</param>
        /// <param name="uv">Boolean toggle for running the method.</param>
        /// <returns>Tuples with horizontal, above, and below neighbor panels.</returns>
        public (IEnumerable<Panel> horizontal, IEnumerable<Panel> above, IEnumerable<Panel> below) GetNeighborCurtainPanels(View view, bool uv)
        {
            BoundingBoxXYZ testPanelBoundingBox = Panel.get_BoundingBox(view);
            IEnumerable<Panel> unSortedPanels = GetNeighborCurtainPanels(view);

            Dictionary<Panel, BoundingBoxXYZ> neighborBoundingBoxes = new Dictionary<Panel, BoundingBoxXYZ>();
            foreach (Panel neighborPanel in unSortedPanels)
            {
                BoundingBoxXYZ bb = neighborPanel.get_BoundingBox(view);
                neighborBoundingBoxes.Add(neighborPanel, bb);
            }

            // Define Horizontal Rules
            IEnumerable<Panel> horizontal = from box in neighborBoundingBoxes
                                            where box.Value.Min.DistanceTo(testPanelBoundingBox.Min) != 0
                                            where ApproximatelyEquals(box.Value.Min.Z, testPanelBoundingBox.Min.Z, 0.25) || ApproximatelyEquals(box.Value.Max.Z, testPanelBoundingBox.Max.Z, 0.25) == true
                                            orderby box.Value.Min.DistanceTo(testPanelBoundingBox.Min) ascending
                                            select box.Key;
            horizontal = horizontal.Take(2);

            // Define Above Rules
            IEnumerable<Panel> above = from box in neighborBoundingBoxes
                                    where box.Value.Min.Z > testPanelBoundingBox.Min.Z && box.Value.Max.Z > testPanelBoundingBox.Max.Z
                                    where ApproximatelyEquals(box.Value.Min.Z, testPanelBoundingBox.Min.Z, 0.05) == false && ApproximatelyEquals(box.Value.Max.Z, testPanelBoundingBox.Max.Z, 0.05) == false
                                    where ApproximatelyEquals(box.Value.Min.X, testPanelBoundingBox.Min.X, 0.10) == true || ApproximatelyEquals(box.Value.Max.X, testPanelBoundingBox.Max.X, 0.10) == true
                                    where ApproximatelyEquals(box.Value.Min.Y, testPanelBoundingBox.Min.Y, 0.10) == true || ApproximatelyEquals(box.Value.Max.Y, testPanelBoundingBox.Max.Y, 0.10) == true
                                    orderby box.Value.Min.DistanceTo(testPanelBoundingBox.Min) ascending
                                    select box.Key;
            above = above.Take(1);

            // Define Below Rules
            IEnumerable<Panel> below = from box in neighborBoundingBoxes
                                    where box.Value.Min.Z < testPanelBoundingBox.Min.Z && box.Value.Max.Z < testPanelBoundingBox.Max.Z
                                    where ApproximatelyEquals(box.Value.Min.Z, testPanelBoundingBox.Min.Z, 0.10) == false && ApproximatelyEquals(box.Value.Max.Z, testPanelBoundingBox.Max.Z, 0.05) == false
                                    where ApproximatelyEquals(box.Value.Min.X, testPanelBoundingBox.Min.X, 0.10) == true || ApproximatelyEquals(box.Value.Max.X, testPanelBoundingBox.Max.X, 0.10) == true
                                    where ApproximatelyEquals(box.Value.Min.Y, testPanelBoundingBox.Min.Y, 0.10) == true || ApproximatelyEquals(box.Value.Max.Y, testPanelBoundingBox.Max.Y, 0.10) == true
                                    orderby box.Value.Min.DistanceTo(testPanelBoundingBox.Min) ascending
                                    select box.Key;
            below = below.Take(1);

            return (horizontal, above, below);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view">Revit View object.</param>
        /// <param name="uiDoc">Revit UIDocument from which to force selection.</param>
        /// <param name="neighborIDs">List of ElementIDs to show.</param>
        public void ShowNeighborCurtainPanels(View view, UIDocument uiDoc, List<ElementId> neighborIDs)
        {
            //ICollection<ElementId> neighborPanels = GetNeighborCurtainPanels(view).Select(e => e.Id).ToList();
            //ICollection<ElementId> neighborPanels = GetNeighborCurtainPanels(view, 4).Select(e => e.Id).ToList();

            uiDoc.Selection.SetElementIds(neighborIDs);
            uiDoc.ShowElements(neighborIDs);
            uiDoc.Selection.Dispose();
        }

        /// <summary>
        /// Exports panel neighboring conditions to a CSV on the desktop. Uses the most refined intersection method in the class.
        /// </summary>
        /// <param name="doc">Revit document object.</param>
        public static void ExportNeighborConditions(Document doc)
        {
            StringBuilder csvContent = new StringBuilder();
            StringBuilder csvDebug = new StringBuilder();
            csvContent.AppendLine("Panel Name,Panel ID,Panel Type,EdgeA,EdgeA ID,EdgeA Type,EdgeB,EdgeB ID,EdgeB Type,Above,Above ID,Above Type,Below,Below ID,Below Type");

            string panelType = "NONE";

            Autodesk.Revit.DB.View view = doc.ActiveView;
            ICollection<Autodesk.Revit.DB.Panel> fullPanelCollection = new FilteredElementCollector(doc, view.Id).OfCategory(BuiltInCategory.OST_CurtainWallPanels)
                                                                            .WhereElementIsNotElementType().Select(p => (Autodesk.Revit.DB.Panel)p).ToList();
            // Iterate over collected panels trying to find neighbors
            foreach (Autodesk.Revit.DB.Panel panel in fullPanelCollection)
            {
                Autodesk.Revit.DB.Panel edgeA = null; string edgeAName = "NONE"; string edgeAid = "0"; string edgeAType = "NONE";
                Autodesk.Revit.DB.Panel edgeB = null; string edgeBName = "NONE"; string edgeBid = "0"; string edgeBType = "NONE";
                Autodesk.Revit.DB.Panel above = null; string aboveName = "NONE"; string aboveId = "0"; string aboveType = "NONE";
                Autodesk.Revit.DB.Panel below = null; string belowName = "NONE"; string belowId = "0"; string belowType = "NONE";

                try
                {
                    CurtainPanelST cST = new CurtainPanelST(panel);
                    var neighborPanels = cST.GetNeighborCurtainPanels(view, true);
                    panelType = panel.Symbol.LookupParameter("Type Mark").AsString();

                    try { edgeA = neighborPanels.horizontal.ToList()[0]; edgeAName = edgeA.Name; edgeAid = edgeA.Id.ToString(); edgeAType = edgeA.Symbol.LookupParameter("Type Mark").AsString(); }
                    catch { csvDebug.AppendLine(panel.Id.ToString() + "," + "EDGE A ERROR"); }

                    try { edgeB = neighborPanels.horizontal.ToList()[1]; edgeBName = edgeB.Name; edgeBid = edgeB.Id.ToString(); edgeBType = edgeB.Symbol.LookupParameter("Type Mark").AsString(); }
                    catch { csvDebug.AppendLine(panel.Id.ToString() + "," + "EDGE CONDITION SIDE"); }

                    try { above = neighborPanels.above.ToList()[0]; aboveName = above.Name; aboveId = above.Id.ToString(); aboveType = above.Symbol.LookupParameter("Type Mark").AsString(); }
                    catch { csvDebug.AppendLine(panel.Id.ToString() + "," + "EDGE CONDITION TOP"); }

                    try { below = neighborPanels.below.ToList()[0]; belowName = below.Name; belowId = below.Id.ToString(); belowType = below.Symbol.LookupParameter("Type Mark").AsString(); }
                    catch { csvDebug.AppendLine(panel.Id.ToString() + "," + "EDGE CONDITION BOTTOM"); }
                }
                catch
                {
                    csvDebug.AppendLine(panel.Id.ToString() + "," + "CRITICAL ERROR");
                }

                // Build CSV data row
                string csvLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}",
                                panel.Name, panel.Id, panelType, edgeAName, edgeAid, edgeAType, edgeBName, edgeBid, edgeBType, aboveName, aboveId, aboveType, belowName, belowId, belowType);
                csvContent.AppendLine(csvLine);

                // Visualize Results in Model
                Parameter parameter = panel.LookupParameter("EDGE CONDITION");
                if (belowType =="NONE" || belowName == "NONE" & parameter != null)
                {
                    parameter.Set("BOTTOM");
                }
                else if (aboveType == "NONE" || aboveName == "NONE" & parameter != null)
                {
                    parameter.Set("TOP");
                }
                else if (edgeBType == "NONE" || edgeBName == "NONE" & parameter != null)
                {
                    parameter.Set("CORNER");
                }
                else
                {
                    parameter.Set("");
                }
            }

            // Export CSV file to desSTop
            string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            File.WriteAllText(dirPath + "\\NEIGHBORdata.csv", csvContent.ToString());
            File.WriteAllText(dirPath + "\\NEIGHBORdata_debug.csv", csvDebug.ToString());
        }
    }
}
