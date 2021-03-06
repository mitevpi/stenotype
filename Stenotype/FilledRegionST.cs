﻿using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
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
    public class FilledRegionST
    {
        /// <summary>
        /// The document to which the Filled Region belongs to.
        /// </summary>
        [NonSerialized()] public readonly Document doc;
        [NonSerialized()] private readonly Application _app;

        /// <summary>
        /// A JSON serialized string representing this class object.
        /// </summary>
        [NonSerialized()] public readonly string Serialized;

        /// <summary>
        /// The JSON object representation of this class.
        /// </summary>
        [NonSerialized()] public JObject JsonObject;

        /// <summary>
        /// The original Filled Region Element used to initialize the class.
        /// </summary>
        [NonSerialized()] public readonly FilledRegion FilledRegion;

        /// <summary>
        /// The name of the Filled Region Element.
        /// </summary>
        [JsonProperty()] public string FilledRegionName { get => FilledRegion.Name.ToString(); set { } }

        /// <summary>
        /// The Element ID of the Filled Region Element.
        /// </summary>
        [NonSerialized()] public readonly ElementId FilledRegionId;

        /// <summary>
        /// The Element ID of the Filled Region Element as an Integer.
        /// </summary>
        [JsonProperty()] public int FilledRegionIdInteger => FilledRegionId.IntegerValue;


        /// <summary>
        /// Initialize with a FilledRegion Object.
        /// </summary>
        /// <param name="filledRegion">A Revit FilledRegion object.</param>
        public FilledRegionST(FilledRegion filledRegion)
        {
            doc = filledRegion.Document;
            _app = doc.Application;
            FilledRegion = filledRegion;
            FilledRegionName = FilledRegion.Name;
            FilledRegionId = FilledRegion.Id;
            Serialized = JsonConvert.SerializeObject(this);
            JsonObject = JObject.Parse(Serialized);
        }

        /// <summary>
        /// Create a temporary undefined Revit Geometry Option necessary for extracting geometry from Revit-wrapped elements.
        /// </summary>
        /// <returns>A Geometry Option object.</returns>
        public Options CreateGeometryOption()
        {
            Options geomOption = _app.Create.NewGeometryOptions();
            geomOption.DetailLevel = ViewDetailLevel.Undefined;
            geomOption.IncludeNonVisibleObjects = false;
            return geomOption;
        }

        /// <summary>
        /// Get the edges of a Filled Region Element.
        /// </summary>
        /// <returns>An Edge Array object.</returns>
        public EdgeArray GetFilledRegionEdges(Options geomOption)
        {
            GeometryElement geometryElement = FilledRegion.get_Geometry(geomOption);
            EdgeArray edgeArray = geometryElement.Select(g => (Solid)g).Select(s => s.Edges) as EdgeArray;

            return edgeArray;
        }

        /// <summary>
        /// Get the GraphicsStyle objects which are applied to the edges of a Filled Region Element.
        /// </summary>
        /// <returns>A list of Revit GraphicsStyle objects.</returns>
        public List<GraphicsStyle> GetFilledRegionLineStyles(Options geomOption)
        {
            List<GraphicsStyle> graphicsStyles = new List<GraphicsStyle>();
            EdgeArray edgeArray = GetFilledRegionEdges(geomOption);

            foreach (Edge edge in edgeArray)
            {
                ElementId elementId = edge.GraphicsStyleId;
                GraphicsStyle lineStyle = doc.GetElement(elementId) as GraphicsStyle;
                graphicsStyles.Add(lineStyle);
            }

            return graphicsStyles;
        }

        /// <summary>
        /// Get the names of the Graphics Styles objects applied to each boundary edge (line) of the Filled Region Element. 
        /// </summary>
        /// <returns>A list of line style names in the form of strings.</returns>
        public List<string> GetFilledRegionLineStyleNames(Options geomOption)
        {
            List<GraphicsStyle> graphicsStyles = GetFilledRegionLineStyles(geomOption);
            List<string> graphicsStyleNames = graphicsStyles.Select(g => g.Name).ToList();

            return graphicsStyleNames;
        }
    }
}
