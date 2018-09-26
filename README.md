![Stenotype Icon](Assets/stenotype_icon.png)
# Stenotype 
Stenotype is a helper class library for working with the Revit API. It consists of .NET helper/extension wrapper classes and methods for writing modular Revit API code faster, and keeping a record. The main code library to reference is the compiled .dll from the C# Stenotype library in this repository.


### Documentation
API documentation for authors is included as .chm help-file: [API Documentation](https://github.com/mitevpi/stenotype/blob/master/Documentation/Help/Stenotype%20API%20Documentation.chm). Documentation created with Sandcastle Help File Builder.

### C# Usage
Typical usage within a C# Revit environment (Visual Studio - [Revit External Command](http://usa.autodesk.com/adsk/servlet/index?siteID=123112&id=20132893) or [Revit Add-In](https://github.com/Andrey-Bushman/Revit2018AddInTemplateSet)).


```c#
using ST = Stenotype;

Options geomOption = ST.FilledRegionsST.CreateGeometryOption() //static usage
ST.FilledRegionsST frST = new ST.FilledRegionsST(doc, fr) //non-static usage

frST.edgeArray; // Class properties return objects
```

```c#
using Newtonsoft.Json;
using Stenotype;

// Create logging JSON object for export or upload
JObject parentJson = new JObject();

// Define Revit document objects (varies depending on implementation)
RevitDoc = this.ActiveUIDocument.Document;
RevitUiDoc = this.ActiveUIDocument;

// Instantiate a Stenotype wrapper class by passing in the Revit API Object as a constructor`
DocumentST docST = new DocumentST(RevitDoc); // Stenotype classes correspond to the Revit API class of the same name + "ST"
Debug.Write(docST.Serialized); // Returns a JSON string
parentJson.Add("Document", docST.JsonObject); // Add the JSON to the parent

// Line Styles 
JObject lineStylesJson = new JObject(); // Create a nested JSON object
Category lineStylesCategory = RevitDoc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines); // Get Document line styles
CategoryNameMap lineStyleSubTypes = lineStylesCategory.SubCategories; // Iterate over the SubCategories
foreach (Category subCatLineStyle in lineStyleSubTypes)
{
    LinestyleST lsST = new LinestyleST(RevitDoc, subCatLineStyle); // Instantiate a Stenotype wrapper class (this one also needs the document as a constructor)
    Debug.Write(lsST.Serialized); // Returns a JSON string
    lineStylesJson.Add(lsST.LineStyleName, lsST.JsonObject); // Add the JSON to the parent
}
parentJson.Add("Line Styles", lineStylesJson); // Add the JSON to the parent

// EXPORT LOGGING JSON
string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); // Set the output directory to the desktop
File.WriteAllText(dirPath + "\\UnitTestData.json", parentJson.ToString()); // Export the parent JSON as a static file
TaskDialog.Show("Export Success", "JSON Exported to Desktop"); // Success

```

### IronPython Usage
Typical usage within an IronPython Revit environment ([RevitPythonShell](https://github.com/architecture-building-systems/revitpythonshell), [pyRevit](https://github.com/eirannejad/pyRevit)).

```python
# IronPython is a .NET lanaguage and != Python/CPython/Python 3
import clr
import sys

# Add the IronPython path to the system path variable 
pyt_path = r'C:\Program Files (x86)\IronPython 2.7\Lib'
sys.path.append(pyt_path)

# Add a .NET reference to the compiled Stenotype .dll
clr.AddReferenceToFileAndPath(r'C:\Users\USERNAME\Documents\Folder\Stenotype.dll')

# Import all classes from the library
from Stenotype import *
```

#### Sample IronPython Usage
Following the import statements above, the class library can used as shown below. For Intellinse/documentaiton, refer to the XML docstrings in the C# source, or complete the imports above in the Interactive Revit Python Shell's console environment (top half of the window).


```python
from Autodesk.Revit.DB import *
doc = __revit__.ActiveUIDocument.Document # Define the Document
filled_region_collection = FilteredElementCollector(doc).OfClass(FilledRegion).WhereElementIsNotElementType().ToElements() # Collect all Filled Regions in the document

filled_region_object = filled_region_collection[0] # Take the first Filled Region as an example

docST = ST.DocumentST(doc) # Instantiate Document class with Document object constructor
frST = ST.FilledRegionsST(doc, filled_region_object) # Instantiate Filled Region class with Filled Region object constructor

print (frST.graphicsStyles) # Some properties hold .NET objects
print (docST.title) # Others hold a string/int representation

print (docST.Serialized) # Returns a JSON string of the public properties - can be exported, or written to a database.
```
