using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Stenotype
{
    /// <summary>
    /// A collection of samples illustrating data export using the Stenotype wrapper classes and methods.
    /// </summary>
    public class Export
    {
        /// <summary>
        /// Export a JSON report of the sheets in the input document. Serialized properties are
        /// pulled from the class properties of the SheetST wrapper class.
        /// </summary>
        /// <param name="doc">The Revit Document object to operate within.</param>
        public static void ExportSheetReport(Document doc)
        {
            // CREATE LOGGING JSON
            JObject parentJson = new JObject();

            // DOCUMENT
            DocumentST docST = new DocumentST(doc);
            parentJson.Add("Document", docST.JsonObject);

            // SHEETS
            JObject sheetsJSON = new JObject();
            ICollection<Element> elements = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Sheets)
                                                                            .WhereElementIsNotElementType()
                                                                            .OrderBy(sht => sht.LookupParameter("Sheet Number").AsString())
                                                                            .ToList();
            foreach (ViewSheet sheet in elements)
            {
                SheetSTX shStx = new SheetSTX(sheet);
                sheetsJSON.Add(shStx.SheetNumber, shStx.JsonObject);
            }
            parentJson.Add("Sheets", sheetsJSON);

            // EXPORT LOGGING JSON
            string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            File.WriteAllText(dirPath + "\\UnitTestData.json", parentJson.ToString());
            TaskDialog.Show("Export Success", "JSON Exported to Desktop");
        }
    }
}
