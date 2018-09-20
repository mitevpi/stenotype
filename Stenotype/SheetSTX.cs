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
    public class SheetSTX : SheetST
    {
        [NonSerialized()] private readonly Document _doc;

        /// <summary>
        /// A list of the Viewports which are on the sheet as Revit Viewport Elements.
        /// </summary>
        [NonSerialized()] public readonly List<Viewport> ViewportElements;

        /// <summary>
        /// A dictionary containing Element IDs of viewports as integers, and their titles as strings.
        /// </summary>
        [JsonProperty()] public readonly Dictionary<int, string> ViewportElementsMap;

        /// <summary>
        /// Viewports which appear on this sheet serialized as JSON objects.
        /// </summary>
        [JsonProperty()] public Dictionary<string, ViewportST> ViewportClasses { get; set; }

        /// <summary>
        /// Initialize with a ViewSheet Object.
        /// </summary>
        /// <param name="sheet">A Revit ViewSheet object.</param>
        public SheetSTX(ViewSheet sheet) : base(sheet)
        {
            ViewportElements = GetViewportElements();
            ViewportElementsMap = GetViewportElementsMap();
            ViewportClasses = GetViewportClasses();
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
