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
    /// Wrapper class for Revit Document Object.</summary>
    /// <remarks>
    /// ADD.
    /// </remarks>
    public class DocumentST
    {
        /// <summary>
        /// A JSON serialized string representing this class object.
        /// </summary>
        [NonSerialized()] public readonly string Serialized;

        /// <summary>
        /// The JSON object representation of this class.
        /// </summary>
        [NonSerialized()] public JObject JsonObject;

        /// <summary>
        /// The document.
        /// </summary>
        [NonSerialized()] public readonly Document Doc;
        [JsonProperty()] private string DocString { get => Doc.Title.ToString(); set { } }

        /// <summary>
        /// The Application hosting the document.
        /// </summary>
        [NonSerialized()] public readonly Application App;
        [JsonProperty()] private string AppString { get => App.ToString(); set { } }

        /// <summary>
        /// The parent UI Application instance.
        /// </summary>
        [NonSerialized()] public readonly UIApplication UiApp;
        [JsonProperty()] private string UiAppString { get => UiApp.ToString(); set { } }

        /// <summary>
        /// The parent UIDocument instance.
        /// </summary>
        [NonSerialized()] public readonly UIDocument UiDoc;
        [JsonProperty()] private string UiDocString { get => UiDoc.ToString(); set { } }

        /// <summary>
        /// The active view of the document at the time of instantiating this class.
        /// </summary>
        [NonSerialized()] public readonly View ActiveView;
        [JsonProperty()] private string ActiveViewString { get => ActiveView.ToString(); set { } }

        /// <summary>
        /// A dictionary of reference types, and reference paths for the files linked to the source Revit document.
        /// </summary>
        [NonSerialized()] public readonly Dictionary<ExternalFileReferenceType, string> RefDict;


        /// <summary>
        /// The Revit Version Number.
        /// </summary>
        public string VersionNumber { get; }
        /// <summary>
        /// The name of the Revit Version.
        /// </summary>
        public string VersionName { get; }
        /// <summary>
        /// The Revit Build Number.
        /// </summary>
        public string VersionBuild { get; }
        /// <summary>
        /// The title of the Revit document.
        /// </summary>
        public string Title { get; }
        /// <summary>
        /// The user name associated with the Revit instance which instantiated this class.
        /// </summary>
        public string UserName { get; }

        /// <summary>
        /// Initialize with a Document object.
        /// </summary>
        /// <param name="doc">Revit Document object.</param>
        public DocumentST(Document doc)
        {
            Doc = doc;
            App = doc.Application;
            UiApp = new UIApplication(App);
            UiDoc = UiApp.ActiveUIDocument;
            Title = doc.Title;
            UserName = UiApp.Application.Username;
            ActiveView = UiDoc.ActiveView;
            VersionNumber = UiApp.Application.VersionNumber;
            VersionName = UiApp.Application.VersionName;
            VersionBuild = UiApp.Application.VersionBuild;
            RefDict = GetExternalReferencePaths();
            Serialized = JsonConvert.SerializeObject(this);
            JsonObject = JObject.Parse(Serialized);
        }

        /// <summary>
        /// Get relevant Revit document objects from a command data input.
        /// </summary>
        /// <param name="externalCommand">ExternalCommandData from an external Revit addin.</param>
        /// <returns> UIApplication, UIDocument, Document, and Application objects.</returns>
        public static (UIApplication uiApp, UIDocument uiDoc, Document doc, Application app) GetDocumentObjects(ExternalCommandData externalCommand)
        {
            UIApplication uiApp = externalCommand.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;
            Application app = doc.Application;

            return (uiApp, uiDoc, doc, app);
        }

        /// <summary>
        /// Get paths of external references associated with the input document.
        /// </summary>
        /// <returns>A dictionary of reference types, and reference paths.</returns>
        public Dictionary<ExternalFileReferenceType, string> GetExternalReferencePaths()
        {
            Dictionary<ExternalFileReferenceType, string> refDict = new Dictionary<ExternalFileReferenceType, string>();
            string location = Doc.PathName;
            try
            {
                ModelPath modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(location);
                TransmissionData transData = TransmissionData.ReadTransmissionData(modelPath);
                ICollection<ElementId> externalReferences = transData.GetAllExternalFileReferenceIds();
                foreach (ElementId elementId in externalReferences)
                {
                    ExternalFileReference extRef = transData.GetLastSavedReferenceData(elementId);
                    ModelPath refPath = extRef.GetPath();
                    string path = ModelPathUtils.ConvertModelPathToUserVisiblePath(refPath);
                    ExternalFileReferenceType refType = extRef.ExternalFileReferenceType;
                    refDict.Add(refType, path);
                }

            }
            catch (Exception exceptionReference) { Console.WriteLine(exceptionReference.ToString()); }

            return refDict;
        }

        /// <summary>
        /// Get a list of line styles loaded in the document as Revit Category objects. These can be iterated over and/or passed to the LineStyleRAW class.
        /// </summary>
        /// <returns>A list of Revit Category objects which represent line styles as settings.</returns>
        public List<Category> GetDocumentLineStyles()
        {
            List<Category> documentLineStyles = new List<Category>();

            Category lineStylesCategory = Doc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines);
            CategoryNameMap lineStyleSubTypes = lineStylesCategory.SubCategories;
            foreach (Category subCatLineStyle in lineStyleSubTypes)
            {
                documentLineStyles.Add(subCatLineStyle);
            }

            return documentLineStyles;
        }

        /// <summary>
        /// Get a JSON representation of the document class.
        /// </summary>
        /// <returns>JSON string with select exposed class properties/attributes.</returns>
        public string SerializeDocumentClass()
        {
            var dRaw = new DocumentST(Doc);
            string jsonString = JsonConvert.SerializeObject(dRaw, Formatting.Indented);
            return jsonString;
        }
    }
}

