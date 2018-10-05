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

all_fams = FilteredElementCollector(doc).WhereElementIsNotElementType().ToElements()

categoryNames = []
for i in all_fams:
	try:
		#print i.Category.Name
		categoryNames.append(i.Category.Name)
	except:
		pass

for i in set(categoryNames):
	print i
