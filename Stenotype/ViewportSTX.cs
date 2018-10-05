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
        /// A list of Element Category Names which will be applied as a filter when collecting elements within the Viewport.
        /// </summary>
        [BsonIgnore] private List<string> IncludedElementsForCollection { get; set; }

        /// <summary>
        /// A class for working with Revit Viewport Elements.
        /// </summary>
        /// <param name="viewport">The Revit Viewport Object.</param>
        public ViewportSTX(Viewport viewport) : base(viewport)
        {   
            // DEPRECATED PROPERTIES
            //ElementsInViewport = GetElementsInViewportByViewType(new List<string> { "ThreeD", "FloorPlan", "Elevation", "Section" });
            //ElementsInViewportClasses = GetElementClasses();

            // SAMPLE PROPERTIES FOR INCLUDING ELEMENT COLLECTION
            //IncludedElementsForCollection = SetIncludedElementCategories();
            //ElementsInViewport = GetElementsInViewportByCategory(IncludedElementsForCollection);
            //ElementsInViewportIds = ElementsInViewport.Select(e => e.Id.IntegerValue).ToList();

            Serialized = JsonConvert.SerializeObject(this);
            JsonObject = JObject.Parse(Serialized);
        }

        private List<string> SetIncludedElementCategories()
        {
            List<string> elementCategoriesToInclude = new List<string>
            {
                "Door Tags", "Window Tags", "Detail Items", "Dimensions", "Door Tags", "Generic Annotations",
                "Automatic Sketch Dimensions", "Lines", "Model Groups", "Room Tags", "Text Notes", "Detail Lines",
                "Keynote Tags"
            };

            return elementCategoriesToInclude;
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
