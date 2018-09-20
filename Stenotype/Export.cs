using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Stenotype
{
    public class Export
    {
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
                SheetST shST = new SheetST(sheet);
                sheetsJSON.Add(shST.SheetNumber, shST.JsonObject);
            }
            parentJson.Add("Sheets", sheetsJSON);

            // EXPORT LOGGING JSON
            string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            File.WriteAllText(dirPath + "\\UnitTestData.json", parentJson.ToString());
            TaskDialog.Show("Export Success", "JSON Exported to Desktop");
        }
    }
}
