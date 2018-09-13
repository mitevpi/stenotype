using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stenotype
{
    /// <summary>
    /// Wrapper class for a Revit Element Object.
    /// </summary>
    /// <remarks>
    /// ADD.
    /// </remarks>
    public class ElementST
    {
        /// <summary>
        /// The document to which the Element belongs to.
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

        [NonSerialized()] public readonly Element Element;
        [JsonProperty()] private string ElementString { get => Element.Name.ToString(); set { } }

        /// <summary>
        /// A dictionary of parameter names, and associated values pulled from the element.
        /// </summary>
        public Dictionary<string, string> ElementParameterValues;

        /// <summary>
        /// Initialize from an Element object.
        /// </summary>
        /// <param name="element">Revit Element object.</param>
        public ElementST(Element element)
        {
            this.Doc = element.Document;
            this.Element = element;
            this.ElementParameterValues = GetElementParameterValues();
            this.Serialized = JsonConvert.SerializeObject(this);
            this.JsonObject = JObject.Parse(Serialized);
        }

        /// <summary>
        /// Get parameter names and their IDs for parameters associated with the element.
        /// </summary>
        /// <returns>A dictionary of Element IDs (as integers) of parameters associated with the element, and their string representation.</returns>
        public Dictionary<int, string> GetElementParameterMap()
        {
            Dictionary<int, string> elementParameterMap = new Dictionary<int, string>();
            ParameterSet elementParameterSet = Element.Parameters;
            foreach (Parameter parameter in elementParameterSet)
            {
                string parameterName = parameter.Definition.Name;
                ElementId parameterId = parameter.Id;
                elementParameterMap.Add(parameterId.IntegerValue, parameterName);
            }
            return elementParameterMap;
        }

        /// <summary>
        /// Get parameter names and their values for parameters associated with the element.
        /// </summary>
        /// <returns>A string dictionary of parameter names, and their values.</returns>
        public Dictionary<string, string> GetElementParameterValues()
        {
            Dictionary<string, string> elementParameterValues = new Dictionary<string, string>();
            ParameterSet elementParameterSet = Element.Parameters;
            foreach (Parameter parameter in elementParameterSet)
            {
                string parameterName = parameter.Definition.Name;
                string parameterType = parameter.StorageType.ToString();
                string parameterValue = "HIDDEN";

                if (parameterType == "ElementId")
                {
                    parameterValue = parameter.AsValueString();
                }
                else if (parameterType == "Integer")
                {
                    parameterValue = parameter.AsInteger().ToString();
                }
                else if (parameterType == "String")
                {
                    parameterValue = parameter.AsString();
                }
                else { }

                if (elementParameterValues.ContainsKey(parameterName)) { }
                else
                {
                    elementParameterValues.Add(parameterName, parameterValue);
                }
            }
            return elementParameterValues;
        }

        /// <summary>
        /// Get a list of elements which intersect the base element's bounding box in a given view.
        /// </summary>
        /// <param name="view">The Revit View within which to run the neighboring element check.</param>
        /// <returns>A list of Revit Element Objects which interset the bounding box of the class-instantiated element.</returns>
        public List<Element> GetNeighborElements(View view)
        {
            // Create bounding box and outline element for the Revit element to check
            BoundingBoxXYZ elementBoundingBox = Element.get_BoundingBox(view);
            Outline elementOutline = new Outline(elementBoundingBox.Min, elementBoundingBox.Max);
            BoundingBoxIntersectsFilter iFilter = new BoundingBoxIntersectsFilter(elementOutline);

            List<Element> neighborElements = new FilteredElementCollector(Doc, view.Id).WhereElementIsNotElementType().WherePasses(iFilter).ToElements().ToList();

            return neighborElements;
        }
    }
}
