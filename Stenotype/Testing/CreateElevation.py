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

# Start a transaction
t = Transaction(doc, "Add Elevation")
t.Start()

eleMarker = ST.UtilST.GetSingleUserSelection(uidoc)

try:
    eleMarker.CreateElevation(doc, doc.ActiveView.Id , 0)
except:
    pass

try:
    eleMarker.CreateElevation(doc, doc.ActiveView.Id , 1)
except:
    pass

try:
    eleMarker.CreateElevation(doc, doc.ActiveView.Id , 2)
except:
    pass

try:
    eleMarker.CreateElevation(doc, doc.ActiveView.Id , 3)
except:
    pass

t.Commit()
t.Dispose()
