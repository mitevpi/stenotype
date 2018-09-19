/*
 * Created by SharpDevelop.
 * User: pmitev
 * Date: 9/5/2018
 * Time: 9:16 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using Stenotype;

namespace TestingFramework
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.DB.Macros.AddInId("7280B4B3-1ABA-43BE-8488-CB07A70B61E1")]
	public partial class ThisApplication
	{
	    private Application RevitApp;
	    private Document RevitDoc;
	    private UIDocument RevitUiDoc;
	    
		# region Module & Startup
		private void Module_Startup(object sender, EventArgs e)
		{

		}

		private void Module_Shutdown(object sender, EventArgs e)
		{

		}

		private void InternalStartup()
		{
			this.Startup += new System.EventHandler(Module_Startup);
			this.Shutdown += new System.EventHandler(Module_Shutdown);
			this.RevitApp = this.Application;
		}
		#endregion
		
		public void StenotypeUnitTest()
		{
		    // CREATE LOGGING JSON
		    JObject parentJson = new JObject();
		    
			// DEFINE DOCUMENT VARIABLES
			RevitDoc = this.ActiveUIDocument.Document;
			RevitUiDoc = this.ActiveUIDocument;
			
			// DOCUMENT TEST
			DocumentST docST = new DocumentST(RevitDoc);
			Debug.Write(docST.Serialized);
			parentJson.Add("Document", docST.JsonObject);
			
			// LINE STYLE TEST
			JObject lineStylesJson = new JObject();
			Category lineStylesCategory = RevitDoc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines);
			CategoryNameMap lineStyleSubTypes = lineStylesCategory.SubCategories;
			foreach (Category subCatLineStyle in lineStyleSubTypes)
            {
				LinestyleST lsST = new LinestyleST(RevitDoc, subCatLineStyle);
				Debug.Write(lsST.Serialized);
				lineStylesJson.Add(lsST.LineStyleName, lsST.JsonObject);
            }
			parentJson.Add("Line Styles", lineStylesJson);
			
			// FILLED REGION TESTER
			JObject filledRegionsJSON = new JObject();
            ICollection<Element> elements = new FilteredElementCollector(RevitDoc).OfClass(typeof(FilledRegion)).WhereElementIsNotElementType().ToElements();
            foreach (FilledRegion region in elements)
            {
                FilledRegionST frST = new FilledRegionST(region);
                Debug.Write(frST.Serialized);
                filledRegionsJSON.Add(frST.FilledRegionName, frST.JsonObject);
            }
            parentJson.Add("Filled Regions", filledRegionsJSON);
            
            // EXPORT LOGGING JSON
		    string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            File.WriteAllText(dirPath + "\\UnitTestData.json", parentJson.ToString());
            TaskDialog.Show("Export Success", "JSON Exported to Desktop");

		}
	}
}