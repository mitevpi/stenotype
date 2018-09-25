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

all_fams = FilteredElementCollector(doc)\
                    .OfClass(Family)\
                    .ToElements()
                    
fam_test = all_fams[1]

for family in all_fams:
	fST = ST.FamilyST(family)
	print fST.Name
	print fST.Serialized