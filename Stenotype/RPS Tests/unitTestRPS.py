import clr
import pprint
import sys

pyt_path = r'C:\Program Files (x86)\IronPython 2.7\Lib'
sys.path.append(pyt_path)
from Autodesk.Revit.DB import *

clr.AddReferenceToFileAndPath( r'C:\Users\pmitev\Documents\GitHub\stenotype\Stenotype\bin\Debug\Stenotype.dll')
import Stenotype as ST

# Set Revit document
doc = __revit__.ActiveUIDocument.Document
uidoc = __revit__.ActiveUIDocument
app = __revit__.Application

#################################
# DOCUMENT TEST
#################################

docST = ST.DocumentST(doc)
pp = pprint.PrettyPrinter(indent=4)
pp.pprint(docST.Serialized)

#################################
# LINE STYLE TEST
#################################
lineStylesCategory = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines)
lineStyleSubTypes = lineStylesCategory.SubCategories

for lineStyle in lineStyleSubTypes:
	testObj = ST.LinestyleST(doc, lineStyle)