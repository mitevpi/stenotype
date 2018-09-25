using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stenotype
{
    public class FamilyST
    {
        [NonSerialized()] private Family _family;

        /// <summary>
        /// A JSON serialized string representing this class object.
        /// </summary>
        [NonSerialized()] public readonly string Serialized;

        /// <summary>
        /// The JSON object representation of this class.
        /// </summary>
        [NonSerialized()] public JObject JsonObject;

        [NonSerialized()] public readonly Document doc;
        [JsonProperty()] private string HostDocument => doc.Title.ToString();

        /// <summary>
        /// The Element ID of the Revit Family Element.
        /// </summary>
        [NonSerialized()] public readonly ElementId ElementId;
        [JsonProperty()] private int ElementIdInteger => ElementId.IntegerValue;

        //[NonSerialized()] public readonly Category Category;
        //[JsonProperty()] private string CategoryString { get => Category.Name ; set { } }

        [NonSerialized()] public readonly Category FamilyCategory;
        [JsonProperty()] private string FamilyCategoryString => FamilyCategory.Name;

        public string Name { get; }
        public long FamilyFileSize { get; }
        public string FamilyCreator { get; }

        public FamilyST(Family family)
        {
            _family = family;
            doc = family.Document;
            Name = family.Name;
            ElementId = family.Id;
            //Category = family.Category;
            FamilyCategory = family.FamilyCategory;
            FamilyFileSize = GetFamilyFileSize();
            FamilyCreator = WorksharingUtils.GetWorksharingTooltipInfo(doc, _family.Id).Creator;
            Serialized = JsonConvert.SerializeObject(this);
            JsonObject = JObject.Parse(Serialized);
        }

        public long GetFamilyFileSize()
        {
            long familySize = 0;

            if (_family.IsEditable == true)
            {
                Document familyDoc = doc.EditFamily(_family);
                string familyPath = familyDoc.PathName;

                if (familyPath != "")
                {
                    familySize = new System.IO.FileInfo(familyPath).Length;
                }

                if (familySize < 1)
                {
                    try
                    {
                        string fileName = _family.Name + ".rfa";
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
    }
}