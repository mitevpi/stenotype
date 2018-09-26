using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stenotype
{
    /// <summary>
    /// Extension of the base ViewportST class for serialization/data export. May include performance-intensive methods.</summary>
    /// <remarks>
    /// TODO.
    /// </remarks>
    public class ViewportSTX: ViewportST
    {
        public ObjectId Id { get; set; }

        /// <summary>
        /// A list of all Revit Elements contained within the Viewport.
        /// </summary>
        [NonSerialized()] [BsonIgnore] public readonly ICollection<Element> ElementsInViewport;
        [JsonProperty()] public List<int> ElementsInViewportIds { get; set; }

        /// <summary>
        /// Viewports which appear on this sheet serialized as JSON objects.
        /// </summary>
        [JsonProperty()] [BsonIgnore] public Dictionary<string, ElementST> ElementsInViewportClasses { get; set; }

        /// <summary>
        /// A class for working with Revit Viewport Elements.
        /// </summary>
        /// <param name="viewport">The Revit Viewport Object.</param>
        public ViewportSTX(Viewport viewport) : base(viewport)
        {
            ElementsInViewport = GetElementsInViewport();
            ElementsInViewportIds = GetElementsInViewport().Select(e => e.Id.IntegerValue).ToList();
            //ElementsInViewportClasses = GetElementClasses();
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
