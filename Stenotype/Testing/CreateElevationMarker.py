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

#####################################

#Get Family View Type
vft = 0
collector = FilteredElementCollector(doc).OfClass(ViewFamilyType).ToElements()

#eleViews = []
for i in collector:
	if i.ViewFamily == ViewFamily.Elevation:		
		vft = i.Id
		break

######################################

roomSelection = ST.UtilST.GetSingleUserSelection(uidoc)
roomObj = ST.RoomST(roomSelection)
roomPoint = roomObj.GetRoomLocation()

roomElements =  roomObj.GetElementsInRoom(doc.ActiveView)
for i in roomElements:
	print i

# Start a transaction
t = Transaction(doc, "Add Elev Marker")
t.Start()

eleMarker = ElevationMarker.CreateElevationMarker(doc, vft, roomPoint, 100)

t.Commit()
t.Dispose()
