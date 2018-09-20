using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stenotype
{
    /// <summary>
    /// Wrapper class for Revit Viewport objects.</summary>
    /// <remarks>
    /// TODO.
    /// </remarks>
    public class ViewportSTX: ViewportST
    {
        [NonSerialized()] private readonly Document _doc;

        /// <summary>
        /// A list of all Revit Elements contained within the Viewport.
        /// </summary>
        [NonSerialized()] public readonly ICollection<Element> ElementsInViewport;

        /// <summary>
        /// Viewports which appear on this sheet serialized as JSON objects.
        /// </summary>
        [JsonProperty()] public Dictionary<string, ElementST> ElementsInViewportClasses { get; set; }

        /// <summary>
        /// A class for working with Revit Viewport Elements.
        /// </summary>
        /// <param name="viewport">The Revit Viewport Object.</param>
        public ViewportSTX(Viewport viewport) : base(viewport)
        {
            _doc = viewport.Document;
            ElementsInViewport = GetElementsInViewport();
            ElementsInViewportClasses = GetElementClasses();
            Serialized = JsonConvert.SerializeObject(this);
            JsonObject = JObject.Parse(Serialized);
        }

        /// <summary>
        /// Get the elements in the viewport as ElementST Class Objects for serialization.
        /// </summary>
        /// <returns>Dictionary containing the Element Name, and the ElementST Class Object.</returns>
        /// TODO: REPLACE ID WITH NAME
        /// TODO: CATEGORIZE IN BUCKET
        /// TODO: LIMIT GATHERING TO USEFUL PARAMETERS
        public Dictionary<string, ElementST> GetElementClasses()
        {
            Dictionary<string, ElementST> elementClassDict = new Dictionary<string, ElementST>();
            foreach (Element element in ElementsInViewport)
            {
                // TODO: if selection.Category.Name == 'Detail Items'
                // TODO: if selection.Category.Name == 'Text Notes'
                // TODO: if selection.Category.Name == 'Detail Lines'
                // TODO: if selection.Category.Name == 'Keynote Tags'


                ElementST elST = new ElementST(element);
                elementClassDict.Add(elST.Element.Id.ToString(), elST);
            }

            return elementClassDict;
        }
    }
}
