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
        /// TESTING NESTED CLASSES
        /// </summary>
        [JsonProperty()] public List<ViewportST> ViewportClasses { get; set; }

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

        public List<ViewportST> GetViewportClasses()
        {
            List<ViewportST> viewportClassList = new List<ViewportST>();
            foreach (Viewport viewportElement in ViewportElements)
            {
                ViewportST vST = new ViewportST(viewportElement);
                viewportClassList.Add(vST);
            }

            return viewportClassList;
        }
    }
}
