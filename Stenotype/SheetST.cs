using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stenotype
{
    /// <summary>
    /// Wrapper class for Revit Filled Region Objects.</summary>
    /// <remarks>
    /// TODO.
    /// </remarks>
    public class SheetST
    {
        [NonSerialized()] private readonly Document _doc;
        [NonSerialized()] public readonly ViewSheet Sheet;

        /// <summary>
        /// A JSON serialized string representing this class object.
        /// </summary>
        [NonSerialized()] public string Serialized;

        /// <summary>
        /// The JSON object representation of this class.
        /// </summary>
        [NonSerialized()] public JObject JsonObject;

        /// <summary>
        /// A list of the Viewports' Element IDs which are on the sheet.
        /// </summary>
        [NonSerialized()] public readonly List<ElementId> ViewportIDs;


        /// <summary>
        /// The Element ID of the Revit Sheet Element.
        /// </summary>
        [NonSerialized()] public readonly ElementId SheetId;
        [JsonProperty()] private int SheetIdInteger { get => SheetId.IntegerValue; set { } }


        /// <summary>
        /// The Sheet title.
        /// </summary>
        public string SheetName { get; }

        /// <summary>
        /// The Sheet number.
        /// </summary>
        public string SheetNumber { get; }

        /// <summary>
        /// The username of the last user to edit the sheet.
        /// </summary>
        public string EditedBy { get; }

        /// <summary>
        /// The Sheet's Approved By parameter value.
        /// </summary>
        public string ApprovedBy { get; }

        /// <summary>
        /// The Sheet's Designed By parameter value.
        /// </summary>
        public string DesignedBy { get; }

        /// <summary>
        /// The Sheet's Checked By parameter value.
        /// </summary>
        public string CheckedBy { get; }

        /// <summary>
        /// The Sheet's Drawn By parameter value.
        /// </summary>
        public string DrawnBy { get; }

        /// <summary>
        /// The Sheet's Issue Date parameter value.
        /// </summary>
        public string IssueDate { get; }

        /// <summary>
        /// Initialize with a ViewSheet Object.
        /// </summary>
        /// <param name="sheet">A Revit ViewSheet object.</param>
        public SheetST(ViewSheet sheet)
        {
            _doc = sheet.Document;
            Sheet = sheet;
            SheetName = Sheet.LookupParameter("Sheet Name").AsString();
            SheetNumber = Sheet.LookupParameter("Sheet Number").AsString();
            SheetId = Sheet.Id;
            EditedBy = Sheet.LookupParameter("Edited by").AsString();
            ApprovedBy = Sheet.LookupParameter("Approved By").AsString();
            DesignedBy = Sheet.LookupParameter("Designed By").AsString();
            CheckedBy = Sheet.LookupParameter("Checked By").AsString();
            DrawnBy = Sheet.LookupParameter("Drawn By").AsString();
            IssueDate = Sheet.LookupParameter("Sheet Issue Date").AsString();
            ViewportIDs = sheet.GetAllViewports().ToList();
            Serialized = JsonConvert.SerializeObject(this);
            JsonObject = JObject.Parse(Serialized);
        }

        /// <summary>
        /// Get the Revit Viewport Elements which are placed on the host Sheet.
        /// </summary>
        /// <returns>A list of Revit Viewport objects.</returns>
        public List<Viewport> GetViewportElements()
        {
            List<Viewport> viewportElements = ViewportIDs.Select(id => _doc.GetElement(id) as Viewport).ToList();

            return viewportElements;
        }

        /// <summary>
        /// Get the Viewports associated with the sheet as a Dictionary of Element IDs as integers, and names of the parent Views as strings.
        /// </summary>
        /// <returns>Keys: Element IDs as integers. Values: View Names as strings.</returns>
        public Dictionary<int, string> GetViewportElementsMap()
        {
            Dictionary<int, string> viewportElementsMap = new Dictionary<int, string>();
            foreach (ElementId viewportId in ViewportIDs)
            {
                Element tempViewportObject = _doc.GetElement(viewportId);
                int tempViewportId = viewportId.IntegerValue;
                string name = tempViewportObject.LookupParameter("View Name").AsString();
                viewportElementsMap.Add(tempViewportId, name);
            }
            return viewportElementsMap;
        }

    }
}
