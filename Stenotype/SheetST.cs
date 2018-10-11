using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using MongoDB.Bson.Serialization.Attributes;
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
        /// <summary>
        /// The original Sheet object used to instantiate the class.
        /// </summary>
        [NonSerialized()] [BsonIgnore] public readonly ViewSheet Sheet;

        /// <summary>
        /// A JSON serialized string representing this class object.
        /// </summary>
        [NonSerialized()] [BsonIgnore] public string Serialized;

        /// <summary>
        /// The JSON object representation of this class.
        /// </summary>
        [NonSerialized()] [BsonIgnore] public JObject JsonObject;

        /// <summary>
        /// A list of the Viewports' Element IDs which are on the sheet.
        /// </summary>
        [NonSerialized()] [BsonIgnore] public readonly List<ElementId> ViewportIds;
        
        /// <summary>
        /// A list of the Viewports' Element IDs which are on the sheet as integers for serialization.
        /// </summary>
        [JsonProperty()] public List<int> ViewportIdIntegers { get => ViewportIds.Select( id => id.IntegerValue).ToList(); set { } }

        /// <summary>
        /// The Document object to which the Sheet belongs to.
        /// </summary>
        [NonSerialized()] [BsonIgnore] public readonly Document Doc;
        
        /// <summary>
        /// The Document object to which the Sheet belongs to as a title (string) for serialization.
        /// </summary>
        [JsonProperty()] public string HostDocument { get => Doc.Title; set { } }

        /// <summary>
        /// The Element ID of the Revit Sheet Element.
        /// </summary>
        [NonSerialized()] [BsonIgnore] public readonly ElementId SheetId;
        
        /// <summary>
        /// The Element ID of the Revit Sheet Element as an integer for serialization.
        /// </summary>
        [JsonProperty()] public int SheetIdInteger { get => SheetId.IntegerValue; set { } }

        /// <summary>
        /// The Sheet title.
        /// </summary>
        public string SheetName { get; set; }

        /// <summary>
        /// The Sheet number.
        /// </summary>
        public string SheetNumber { get; set; }

        /// <summary>
        /// The username of the last user to edit the sheet.
        /// </summary>
        public string EditedBy { get; set; }

        /// <summary>
        /// The Sheet's Approved By parameter value.
        /// </summary>
        public string ApprovedBy { get; set; }

        /// <summary>
        /// The Sheet's Designed By parameter value.
        /// </summary>
        public string DesignedBy { get; set; }

        /// <summary>
        /// The Sheet's Checked By parameter value.
        /// </summary>
        public string CheckedBy { get; set; }

        /// <summary>
        /// The Sheet's Drawn By parameter value.
        /// </summary>
        public string DrawnBy { get; set; }

        /// <summary>
        /// The Sheet's Issue Date parameter value.
        /// </summary>
        public string IssueDate { get; set; }

        /// <summary>
        /// Initialize with a ViewSheet Object.
        /// </summary>
        /// <param name="sheet">A Revit ViewSheet object.</param>
        public SheetST(ViewSheet sheet)
        {
            Doc = sheet.Document;
            Sheet = sheet;
            SheetName = Sheet.LookupParameter("Sheet Name").AsString();
            SheetNumber = Sheet.LookupParameter("Sheet Number").AsString();
            SheetId = Sheet.Id;
            //EditedBy = Sheet.LookupParameter("Edited by").AsString();
            ApprovedBy = Sheet.LookupParameter("Approved By").AsString();
            DesignedBy = Sheet.LookupParameter("Designed By").AsString();
            CheckedBy = Sheet.LookupParameter("Checked By").AsString();
            DrawnBy = Sheet.LookupParameter("Drawn By").AsString();
            IssueDate = Sheet.LookupParameter("Sheet Issue Date").AsString();
            ViewportIds = sheet.GetAllViewports().ToList();
            Serialized = JsonConvert.SerializeObject(this);
            JsonObject = JObject.Parse(Serialized);
        }

        /// <summary>
        /// Get the Revit Viewport Elements which are placed on the host Sheet.
        /// </summary>
        /// <returns>A list of Revit Viewport objects.</returns>
        public List<Viewport> GetViewportElements()
        {
            List<Viewport> viewportElements = ViewportIds.Select(id => Doc.GetElement(id) as Viewport).ToList();

            return viewportElements;
        }

        /// <summary>
        /// Get the Viewports associated with the sheet as a Dictionary of Element IDs as integers, and names of the parent Views as strings.
        /// </summary>
        /// <returns>Keys: Element IDs as integers. Values: View Names as strings.</returns>
        public Dictionary<int, string> GetViewportElementsMap()
        {
            Dictionary<int, string> viewportElementsMap = new Dictionary<int, string>();
            foreach (ElementId viewportId in ViewportIds)
            {
                Element tempViewportObject = Doc.GetElement(viewportId);
                int tempViewportId = viewportId.IntegerValue;
                string name = tempViewportObject.LookupParameter("View Name").AsString();
                viewportElementsMap.Add(tempViewportId, name);
            }
            return viewportElementsMap;
        }

    }
}
