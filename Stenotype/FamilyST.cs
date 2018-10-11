using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stenotype
{
    /// <summary>
    /// Wrapper class for Revit Family objects.</summary>
    /// <remarks>
    /// TODO.
    /// </remarks>
    public class FamilyST
    {

        /// <summary>
        /// The Family object used to instantiate the class.
        /// </summary>
        [NonSerialized()] [BsonIgnore] public Family Family;

        /// <summary>
        /// A JSON serialized string representing this class object.
        /// </summary>
        [NonSerialized()] [BsonIgnore] public string Serialized;

        /// <summary>
        /// The JSON object representation of this class.
        /// </summary>
        [NonSerialized()] [BsonIgnore] public JObject JsonObject;

        /// <summary>
        /// The Revit Document object to which the Family belongs.
        /// </summary>
        [NonSerialized()] [BsonIgnore] public readonly Document doc;

        /// <summary>
        /// The Revit Document object to which the Family belongs as a title (string) for serialization.
        /// </summary>
        [JsonProperty()] public string HostDocument { get => doc.Title.ToString(); set { } }

        /// <summary>
        /// The Element ID of the Revit Family Element.
        /// </summary>
        [NonSerialized()] [BsonIgnore] public readonly ElementId ElementId;

        /// <summary>
        /// The Element ID of the Revit Family Element as an integer for serialization.
        /// </summary>
        [JsonProperty()] public int ElementIdInteger { get => ElementId.IntegerValue; set { } }

        //[NonSerialized()] public readonly Category Category;
        //[JsonProperty()] private string CategoryString { get => Category.Name ; set { } }

        /// <summary>
        /// The Category to which the Family object belongs to.
        /// </summary>
        [NonSerialized()] [BsonIgnore] public readonly Category FamilyCategory;

        /// <summary>
        /// The Category to which the Family object belongs to as a name (string) for serialization.
        /// </summary>
        [JsonProperty()] public string FamilyCategoryString { get => FamilyCategory.Name; set { } }

        /// <summary>
        /// A dictionary of parameter names, and associated values pulled from the element.
        /// </summary>
        /// TODO: WRITE CUSTOM CSV EXPORTER FROM MONGO TO IGNORE 
        [BsonIgnore] public Dictionary<string, string> FamilyParameterValues { get; set; }

        /// <summary>
        /// The name of the Family.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The username of the author of the Family, if there is one.
        /// </summary>
        public string FamilyCreator { get; set; }

        /// <summary>
        /// The number of Family types that are available for this family.
        /// </summary>
        public int FamilyTypesCount { get; set; }

        /// <summary>
        /// Whether or not this Family has any parametric relationships inside.
        /// </summary>
        public bool IsParametricFamily { get; set; }

        /// <summary>
        /// Whether or not the Family is modeled in place, or created in the Family editor and loaded in.
        /// </summary>
        public bool IsModeledInPlaceFamily { get; set; }

        /// <summary>
        /// A class for working with Revit Family Elements.
        /// </summary>
        /// <param name="family">A Revit Family object.</param>
        public FamilyST(Family family)
        {
            Family = family;
            doc = family.Document;
            Name = family.Name;
            ElementId = family.Id;
            //Category = family.Category;
            FamilyCategory = family.FamilyCategory;
            FamilyParameterValues = GetFamilyParameterValues();
            FamilyTypesCount = family.GetFamilySymbolIds().Count;
            IsParametricFamily = family.IsParametric;
            IsModeledInPlaceFamily = family.IsInPlace;
            FamilyCreator = WorksharingUtils.GetWorksharingTooltipInfo(doc, Family.Id).Creator;
            Serialized = JsonConvert.SerializeObject(this);
            JsonObject = JObject.Parse(Serialized);
        }

        /// <summary>
        /// Try to get the file size of a Revit Family. If the Family is originally loaded in locally, the
        /// file size will be found through the path name of the Family. If not (which is most of the time), the
        /// Families will be exported to the desktop, and file sizes will be harvested from that directory.
        /// </summary>
        /// <returns></returns>
        public long GetFamilyFileSize()
        {
            long familySize = 0;

            if (Family.IsEditable == true)
            {
                Document familyDoc = doc.EditFamily(Family);
                string familyPath = familyDoc.PathName;

                if (familyPath != "")
                {
                    familySize = new System.IO.FileInfo(familyPath).Length;
                }

                if (familySize < 1)
                {
                    try
                    {
                        string fileName = Family.Name + ".rfa";
                        string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Quarry";
                        System.IO.Directory.CreateDirectory(dirPath);
                        string familyPathNew = dirPath + "\\" + fileName;

                        if (File.Exists(familyPathNew))
                        {
                            familySize = new System.IO.FileInfo(familyPathNew).Length;
                        }
                        else
                        {
                            familyDoc.SaveAs(familyPathNew);
                            familyDoc.Close();
                            familySize = new System.IO.FileInfo(familyPathNew).Length;
                        }
                    }
                    catch
                    {
                        familySize = 0;
                    }
                }
            }
            return familySize;
        }

        /// <summary>
        /// Get parameter names and their values for parameters associated with the Family.
        /// </summary>
        /// <returns>A string dictionary of parameter names, and their values.</returns>
        public Dictionary<string, string> GetFamilyParameterValues()
        {
            Dictionary<string, string> familyParameterValues = new Dictionary<string, string>();
            ParameterSet elementParameterSet = Family.Parameters;
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

                if (familyParameterValues.ContainsKey(parameterName)) { }
                else
                {
                    familyParameterValues.Add(parameterName, parameterValue);
                }
            }
            return familyParameterValues;
        }

        /// <summary>
        /// Get all the placed instances of this Family.
        /// </summary>
        /// <returns>A list of FamilyInstance objects.</returns>
        public List<FamilyInstance> GetFamilyInstances()
        {
            List<FamilyInstance> familyInstances = new FilteredElementCollector(doc)
                                                    .OfClass(typeof(FamilyInstance))
                                                    .Select(f => (FamilyInstance)f)
                                                    .Where(x => x.Symbol.Family.Name == Name || x.Name == Name)
                                                    .ToList();

            return familyInstances;
        }
    }
}