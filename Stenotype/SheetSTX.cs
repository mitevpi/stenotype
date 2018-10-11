using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stenotype
{
    /// <summary>
    /// Extension of the base SheetST class for serialization/data export. May include performance-intensive methods.</summary>
    /// <remarks>
    /// TODO.
    /// </remarks>
    public class SheetSTX : SheetST
    {
        /// <summary>
        /// ID parameter for MongoDB interaction.
        /// </summary>
        public ObjectId Id { get; set; }

        /// <summary>
        /// A list of the Viewports which are on the sheet as Revit Viewport Elements.
        /// </summary>
        [NonSerialized()] [BsonIgnore] public readonly List<Viewport> ViewportElements;

        /// <summary>
        /// A dictionary containing Element IDs of viewports as integers, and their titles as strings.
        /// </summary>
        [JsonProperty()] [BsonIgnore] public readonly Dictionary<int, string> ViewportElementsMap;

        /// <summary>
        /// Viewports which appear on this sheet serialized as JSON objects.
        /// </summary>
        [JsonProperty()] [BsonIgnore] public Dictionary<string, ViewportST> ViewportClasses { get; set; }

        /// <summary>
        /// A list of the titles of each View on the sheet.
        /// </summary>
        public List<string> ViewTitles { get; set; }

        /// <summary>
        /// Initialize with a ViewSheet Object.
        /// </summary>
        /// <param name="sheet">A Revit ViewSheet object.</param>
        public SheetSTX(ViewSheet sheet) : base(sheet)
        {
            ViewportElements = GetViewportElements();
            ViewportElementsMap = GetViewportElementsMap();
            //ViewTitles = ViewportElementsMap.Values.ToList();
            //ViewportClasses = GetViewportClasses();
            Serialized = JsonConvert.SerializeObject(this);
            JsonObject = JObject.Parse(Serialized);
        }

        /// <summary>
        /// Get the viewports on the sheet as ViewportST Class Objects for serialization.
        /// </summary>
        /// <returns>Dictionary containing the View Name, and the ViewportST Class Object.</returns>
        public Dictionary<string, ViewportST> GetViewportClasses()
        {
            Dictionary<string, ViewportST> viewportClassDict = new Dictionary<string, ViewportST>();
            foreach (Viewport viewportElement in ViewportElements)
            {
                ViewportSTX vSTX = new ViewportSTX(viewportElement);
                viewportClassDict.Add(vSTX.ViewName ,vSTX);
            }

            return viewportClassDict;
        }
    }
}
