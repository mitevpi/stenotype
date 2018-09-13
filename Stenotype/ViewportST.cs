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
    public class ViewportST
    {
        [NonSerialized()] private readonly Document _doc;
        [NonSerialized()] public readonly Viewport Viewport;

        /// <summary>
        /// The Revit View element which is being represented through this Viewport.
        /// </summary>
        [NonSerialized()] public readonly View View;

        /// <summary>
        /// A list of all Revit Elements contained within the Viewport.
        /// </summary>
        [NonSerialized()] public ICollection<Element> ViewportElements;

        /// <summary>
        /// A JSON serialized string representing this class object.
        /// </summary>
        [NonSerialized()] public readonly string Serialized;

        /// <summary>
        /// The JSON object representation of this class.
        /// </summary>
        [NonSerialized()] public JObject JsonObject;

        /// <summary>
        /// The ElementID of the Viewport Element.
        /// </summary>
        [NonSerialized()] public readonly ElementId ViewportId;
        [JsonProperty()] private int ViewportIdInteger => ViewportId.IntegerValue;

        /// <summary>
        /// The Element ID of the Viewport Owner element.
        /// </summary>
        [NonSerialized()] public readonly ElementId ViewOwnerViewId;
        [JsonProperty()] private int ViewOwnerViewIdInteger => ViewOwnerViewId.IntegerValue;

        /// <summary>
        /// The Element ID of the View Element that is being represented through this Viewport.
        /// </summary>
        [NonSerialized()] public readonly ElementId ViewId;
        [JsonProperty()] private int ViewIdInteger => ViewId.IntegerValue;

        /// <summary>
        /// The name/title of the Viewport Element.
        /// </summary>
        public string ViewportName { get; }

        /// <summary>
        /// The name/title of the View which is represented through this Viewport.
        /// </summary>
        public string ViewName { get; }

        /// <summary>
        /// The name/title of the View Template associated with the viewport.
        /// </summary>
        public string ViewTemplate { get; }

        /// <summary>
        /// The title of the view on the sheet that is being represented through this Viewport.
        /// </summary>
        public string ViewTitleOnSheet { get; }

        /// <summary>
        /// The scale of the view that is being represented through this Viewport.
        /// </summary>
        public string ViewScale { get; }

        /// <summary>
        /// The type of view that is being represented through this Viewport.
        /// </summary>
        public string ViewType { get; }

        /// <summary>
        /// The detail level of the view that is being represented through this Viewport.
        /// </summary>
        public string ViewDetailLevel { get; }

        /// <summary>
        /// A class for working with Revit Viewport Elements.
        /// </summary>
        /// <param name="viewport">The Revit Viewport Object.</param>
        public ViewportST(Viewport viewport)
        {
            _doc = viewport.Document;
            Viewport = viewport;
            ViewportName = viewport.Name;
            ViewportId = viewport.Id;
            ViewName = viewport.LookupParameter("View Name").AsString();
            ViewTemplate = viewport.LookupParameter("View Template").AsString();
            ViewTitleOnSheet = viewport.LookupParameter("Title on Sheet").AsString();
            ViewOwnerViewId = viewport.OwnerViewId;
            ViewId = viewport.ViewId;
            View = _doc.GetElement(ViewId) as View;
            ViewScale = View.Scale.ToString();
            ViewType = View.ViewType.ToString();
            ViewDetailLevel = View.DetailLevel.ToString();
            ViewportElements = GetElementsInViewport();
            Serialized = JsonConvert.SerializeObject(this);
            JsonObject = JObject.Parse(Serialized);
        }

        /// <summary>
        /// Get the Revit Elements which appear within the boudning box of the viewport.
        /// </summary>
        /// <returns>An ICollection of Revit Element objects.</returns>
        public ICollection<Element> GetElementsInViewport()
        {
            ICollection<Element> viewportElements = new FilteredElementCollector(_doc, ViewId).WhereElementIsNotElementType().ToElements();
            return viewportElements;
        }

        /// <summary>
        /// Get callouts which refer to this element.
        /// </summary>
        /// <returns>A list of Revit Element Objects.</returns>
        public List<Element> GetReferringCallouts()
        {
            List<Element> callouts = new FilteredElementCollector(_doc).OfCategory(BuiltInCategory.OST_Viewers).ToElements().Where(callout => callout.Name == ViewName).ToList();
            return callouts;
        }


        /// <summary>
        /// A "replacement" for Get Referring Views, since there is no API equivalent. Currently very brute-force. Needs improvement.
        /// </summary>
        /// <returns>A list of Revit Element Objects.</returns>
        public List<Element> GetReferringViews()
        {
            List<Element> referringViews = new List<Element>();
            Element calloutElement = GetReferringCallouts()[0];

            ICollection<Element> viewElementsUncast = new FilteredElementCollector(_doc).OfClass(typeof(View)).WhereElementIsNotElementType().ToElements();
            List<View> viewElements = viewElementsUncast.Select(ve => (View)ve).ToList();

            foreach (View viewElement in viewElements)
            {
                try
                {
                    // https://goo.gl/x8ReQb
                    BoundingBoxXYZ bb = calloutElement.get_BoundingBox(viewElement);
                    if (bb != null)
                    {
                        referringViews.Add(viewElement);
                    }
                }
                catch (Exception boundingBoxException)
                {
                    //Debug.Print(boundingBoxException.Message);
                }
            }

            return referringViews;
        }

        /// <summary>
        /// WORK IN PROGRESS. USE NON OVERLOAD METHOD. A "replacement" for Get Referring Views, since there is no API equivalent. Currently very brute-force. Needs improvement.
        /// </summary>
        /// <returns>A list of Revit Element Objects.</returns>
        public List<Element> GetReferringViews(bool toggle)
        {
            List<Element> referringViews = new List<Element>();
            Element calloutElement = GetReferringCallouts()[0];

            ICollection<Element> viewElementsUncast = new FilteredElementCollector(_doc).OfClass(typeof(View)).WhereElementIsNotElementType().ToElements();
            List<View> viewElements = viewElementsUncast.Select(ve => (View)ve).ToList();

            foreach (View viewElement in viewElements)
            {
                try
                {
                    List<Element> tempCollection = new FilteredElementCollector(_doc, viewElement.Id).WhereElementIsNotElementType().Where(element => element.Name == calloutElement.Name).ToList();
                    if (tempCollection.Any())
                    {
                        referringViews.Add(viewElement);
                    }
                }
                catch (Exception boundingBoxException)
                {
                    //Debug.Print(boundingBoxException.Message);
                }
            }

            return referringViews;
        }
    }
}
