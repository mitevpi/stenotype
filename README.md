![Stenotype Icon](Assets/stenotype_icon.png | width=150)
# Stenotype 
Stenotype is a helper class library for working with the Revit API. It consists of .NET helper/extension wrapper classes and methods for writing modular Revit API code, faster (and only once). The classes can be used for any custom Revit tools/scripts. The main code library to reference is the compiled .dll from the C# Stenotype library in this repository.

### Debugging
IronPython debugging can be done through [RevitPythonShell](https://github.com/architecture-building-systems/revitpythonshell), via the python script included in this repo. 
C# debugging: COMING SOON.

### Documentation
API documentation for authors: COMING SOON.

### C# Usage
Typical usage within a C# Revit environment (Visual Studio - [Revit External Command](http://usa.autodesk.com/adsk/servlet/index?siteID=123112&id=20132893) or [Revit Add-In](https://github.com/Andrey-Bushman/Revit2018AddInTemplateSet)).


```c#
using ST = Stenotype;

Options geomOption = ST.FilledRegionsST.CreateGeometryOption() //static usage
ST.FilledRegionsST frST = new ST.FilledRegionsST(doc, fr) //non-static usage

frST.edgeArray; // Class properties return objects
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
```
