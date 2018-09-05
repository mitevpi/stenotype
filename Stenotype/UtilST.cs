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
    /// General utility static functions and methods.
    /// </summary>
    /// <remarks>
    /// TODO.
    /// </remarks>
    public class UtilST
    {
        /// <summary>
        /// Serialize the custom RevitApiWrapper Object to a JSON String.
        /// </summary>
        /// <param name="classObject">A RevitApiWrapper Object.</param>
        /// <returns></returns>
        public static string SerializeClass(object classObject)
        {
            string jsonString = JsonConvert.SerializeObject(classObject, Formatting.Indented);
            return jsonString;
        }

        /// <summary>
        /// Get a single item from a user's mouse selection in Revit (preferably only one item is selected).
        /// </summary>
        /// <param name="uiDoc">The UIDocument object from which to pull user selection.</param>
        /// <returns>A Revit Element object.</returns>
        public static Element GetSingleUserSelection(UIDocument uiDoc)
        {
            List<ElementId> selectionId = uiDoc.Selection.GetElementIds().ToList();
            Element element = uiDoc.Document.GetElement(selectionId[0]);

            return element;
        }

        /// <summary>
        /// Get the first X amount of items from a user's selection.
        /// </summary>
        /// <param name="uiDoc">The UIDocument object from which to pull user selection.</param>
        /// <returns>List of Revit Element objects.</returns>
        public static List<Element> GetMultpleUserSelection(UIDocument uiDoc, int numberOfSelections)
        {
            List<ElementId> selectionIds = uiDoc.Selection.GetElementIds().Take(numberOfSelections).ToList();
            List<Element> selectedElements = selectionIds.Select(id => uiDoc.Document.GetElement(id)).ToList();

            return selectedElements;
        }

        /// <summary>
        /// Get all itmes from a user's selection.
        /// </summary>
        /// <param name="uiDoc">The UIDocument object from which to pull user selection.</param>
        /// <returns>List of Revit Element objects.</returns>
        public static List<Element> GetMultpleUserSelection(UIDocument uiDoc)
        {
            List<ElementId> selectionIds = uiDoc.Selection.GetElementIds().ToList();
            List<Element> selectedElements = selectionIds.Select(id => uiDoc.Document.GetElement(id)).ToList();

            return selectedElements;
        }

        /// <summary>
        /// Create a temporary undefined Revit Geometry Option necessary for extracting geometry from Revit-wrapped elements.
        /// </summary>
        /// <returns>A Geometry Option object.</returns>
        public static Options CreateGeometryOption(Application app)
        {
            Options geomOption = app.Create.NewGeometryOptions();
            geomOption.DetailLevel = ViewDetailLevel.Undefined;
            geomOption.IncludeNonVisibleObjects = false;
            return geomOption;
        }
    }
}

