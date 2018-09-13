using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stenotype
{
    /// <summary>
    /// Wrapper class for Revit "Linestyles" (Category objects).
    /// A Linestyle is a Document Setting in Revit, not a typical object. The Category object contains the Linestyle information
    /// </summary>
    public class LinestyleST
    {
        /// <summary>
        /// The Document to which the Category belongs to.
        /// </summary>
        [NonSerialized()] public readonly Document Doc;

        /// <summary>
        /// The Category object which holds the linestyles.
        /// </summary>
        [NonSerialized()] public readonly Category SubCategoryLinestyle;

        /// <summary>
        /// The JSON string representation of this class object.
        /// </summary>
        [NonSerialized()] public readonly string Serialized;

        /// <summary>
        /// The JSON object representation of this class.
        /// </summary>
        [NonSerialized()] public JObject JsonObject;

        /// <summary>
        /// The Revit Element representing the line pattern.
        /// </summary>
        [NonSerialized()] public readonly LinePatternElement LinePatternElement;

        /// <summary>
        /// The LinePattern parent class associated with the LinePatternElement applied to this linestyle.
        /// </summary>
        [NonSerialized()] public readonly LinePattern LinePattern;

        /// <summary>
        /// The ElementID of the linestyle.
        /// </summary>
        [NonSerialized()] public readonly ElementId LineStyleId;
        [JsonProperty()] private int LineStyleIdInteger => LineStyleId.IntegerValue;

        /// <summary>
        /// The Element ID of the line pattern element.
        /// </summary>
        [NonSerialized()] public readonly ElementId LinePatternId;
        [JsonProperty()] private int LinePatternIdInteger => LinePatternId.IntegerValue;


        /// <summary>
        /// The assigned name of the line style ast it appears Revit.
        /// </summary>
        public string LineStyleName { get; }

        /// <summary>
        /// The weight of the line style as an integer.
        /// </summary>
        public string LineStyleWeight { get; }

        /// <summary>
        /// An ordered list of numbers representing RGB values of the line style color.
        /// </summary>
        public List<int> LineStyleRgb { get; }

        /// <summary>
        /// The name of the line pattern applied to the line style.
        /// </summary>
        public string LinePatternName { get; }

        /// <summary>
        /// A list of the lengths of individual components of the line pattern as doubles.
        /// </summary>
        public List<double> LinePatternComponentsLength { get; }

        /// <summary>
        ///  A list of the types of line elements which make up individual components of the line pattern.
        /// </summary>
        public List<string> LinePatternComponentsType { get; }


        /// <summary>
        /// A Linestyle is a Document Setting in Revit, not a typical object. The Category object contains the Linestyle information.
        /// </summary>
        /// <param name="doc">The Revit Document object.</param>
        /// <param name="subCategoryLinestyle">Revit Category object.</param>
        public LinestyleST(Document doc, Category subCategoryLinestyle)
        {
            Doc = doc;
            SubCategoryLinestyle = subCategoryLinestyle;
            LineStyleName = SubCategoryLinestyle.Name;
            LineStyleId = SubCategoryLinestyle.Id;
            LineStyleWeight = SubCategoryLinestyle.GetLineWeight(GraphicsStyleType.Projection).ToString();
            LineStyleRgb = GetLineStyleRgb();
            LinePatternId = GetLinePatternId();
            LinePatternName = GetLinePatternName();
            LinePatternElement = GetLinePatternElement();
            LinePattern = GetLinePattern();
            LinePatternComponentsLength = GetLinePatternSegmentLengths();
            LinePatternComponentsType = GetLinePatternSegmentTypes();
            Serialized = JsonConvert.SerializeObject(this);
            JsonObject = JObject.Parse(Serialized);
        }

        /// <summary>
        /// Get an ordered list of integer values corresponding to the RGB values of the line style.
        /// </summary>
        /// <returns>Ordered list of RGB integers.</returns>
        public List<int> GetLineStyleRgb()
        {
            // COLOR PARSING
            int lineStyleRed = 0;
            int lineStyleGreen = 0;
            int lineStyleBlue = 0;

            // Eliminate missing, null, or otherwise void color objects.
            try
            {
                lineStyleRed = int.Parse(SubCategoryLinestyle.LineColor.Red.ToString());
                lineStyleGreen = int.Parse(SubCategoryLinestyle.LineColor.Green.ToString());
                lineStyleBlue = int.Parse(SubCategoryLinestyle.LineColor.Blue.ToString());
                 

                if (Enumerable.Range(0, 255).Contains(lineStyleRed)) { }
                else
                    lineStyleRed = 0;

                if (Enumerable.Range(0, 255).Contains(lineStyleGreen)) { }
                else
                    lineStyleGreen = 0;

                if (Enumerable.Range(0, 255).Contains(lineStyleBlue)) { }
                else
                    lineStyleBlue = 0;

            }
            catch (Exception exceptionColor) { Console.WriteLine(exceptionColor.ToString()); }

            // Create a list of RGB integer values.
            List<int> lineStyleRgb = new List<int>(new[] { lineStyleRed, lineStyleGreen, lineStyleBlue });

            return lineStyleRgb;
        }

        /// <summary>
        /// Get the Element ID of the Line Pattern object.
        /// </summary>
        /// <returns>Element ID of the Line Pattern object.</returns>
        public ElementId GetLinePatternId()
        {
            ElementId linePatternId = null;

            // Account for deprecated API methods and missing objects from "Solid" line patterns.
            try
            {
                linePatternId = SubCategoryLinestyle.GetLinePatternId(GraphicsStyleType.Projection);
            }
            catch (Exception exceptionPattern) { Console.WriteLine(exceptionPattern.ToString()); }

            return linePatternId;
        }

        /// <summary>
        /// Get the Revit Object representation of the Line Pattern Element.
        /// </summary>
        /// <returns>LinePatternElement object</returns>
        public LinePatternElement GetLinePatternElement()
        {
            LinePatternElement patternElement = null;

            // Account for deprecated API methods and missing objects from "Solid" line patterns.
            try
            {
                patternElement = Doc.GetElement(LinePatternId) as LinePatternElement;
            }
            catch (Exception exceptionPattern) { Console.WriteLine(exceptionPattern.ToString()); }

            return patternElement;
        }

        /// <summary>
        /// Get the name of the line pattern.
        /// </summary>
        /// <returns>Name of the line pattern.</returns>
        public string GetLinePatternName()
        {
            string linePatternName;

            if (LinePatternElement != null)
            {
                try
                {
                    linePatternName = LinePatternElement.Name.ToString();
                }
                catch (Exception exceptionPattern)
                {
                    linePatternName = "Solid";
                    Console.WriteLine(exceptionPattern.ToString());
                }
            }
            else
            {
                linePatternName = "Solid";
            }

            return linePatternName;
        }

        /// <summary>
        /// Retrieve the parent LinePattern object to the lower-level LinePatternElement object.
        /// </summary>
        /// <returns>The Revit LinePattern object (not LinePatternElement).</returns>
        public LinePattern GetLinePattern()
        {
            LinePattern linePattern = null;

            try
            {
                linePattern = LinePatternElement.GetLinePattern();
            }
            catch (Exception exceptionLinePattern) { Console.WriteLine(exceptionLinePattern.ToString()); }

            return linePattern;
        }

        /// <summary>
        /// Get an ordered list of doubles representing the lengths of individual components of a line pattern.
        /// </summary>
        /// <returns>Ordered list of segment lenghts.</returns>
        public List<double> GetLinePatternSegmentLengths()
        {
            List<double> linePatternSegmentLengths = new List<double>();

            if (LinePattern != null)
            {
                linePatternSegmentLengths = LinePattern.GetSegments().ToList().Select(sg => sg.Length).ToList();
            }

            return linePatternSegmentLengths;
        }

        /// <summary>
        /// Get an ordered list of line types representing the type of line in each individual component of a line pattern (solid vs dashed, etc.).
        /// </summary>
        /// <returns>Ordered list of segment types.</returns>
        public List<string> GetLinePatternSegmentTypes()
        {
            List<string> linePatternSegmentTypes = new List<string>();

            if (LinePattern != null)
            {
                linePatternSegmentTypes = LinePattern.GetSegments().ToList().Select(sg => sg.Type.ToString()).ToList();
            }

            return linePatternSegmentTypes;
        }
    }
}
