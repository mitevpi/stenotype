#Copyright(c) 2016 www.Learndynamo.com 
#Please contact at jeremy@learndynamo.com

import clr
clr.AddReference('RevitAPI')
clr.AddReference("RevitServices")
clr.AddReference("RevitNodes")
import RevitServices
import Revit
import Autodesk
from Autodesk.Revit.DB import *
from math import *

clr.ImportExtensions(Revit.GeometryConversion)

from RevitServices.Persistence import DocumentManager
from RevitServices.Transactions import TransactionManager

doc = DocumentManager.Instance.CurrentDBDocument

toggle = IN[0]
points = UnwrapElement(IN[1])
modelPoints = UnwrapElement(IN[2])
cropCurves = UnwrapElement(IN[3])
viewType = UnwrapElement(IN[4])

lst = []

#Get Family View Type
vft = 0
collector = FilteredElementCollector(doc).OfClass(ViewFamilyType).ToElements()

#eleViews = []
for i in collector:
	if i.ViewFamily == ViewFamily.Elevation:		
		vft = i.Id
		break
 
if toggle == True:
	
	TransactionManager.Instance.EnsureInTransaction(doc)
	
	for ind, point in enumerate(points):
	
		#Retrieve the mid point of model lines and get X,Y.		
		modelMP = modelPoints[ind].ToXyz()
		modelMPX = modelMP.X
		modelMPY = modelMP.Y
		
		#Retrieve individual lines of crop window.		
		cropLines = cropCurves[ind]
		l1 = cropLines[0].ToRevitType()
		l2 = cropLines[1].ToRevitType()
		l3 = cropLines[2].ToRevitType()
		l4 = cropLines[3].ToRevitType()
					
		# Create a line in the z-Axis for elevation marker to rotate around.			
		elevationPT = point.ToXyz()
		elptRotate = XYZ(elevationPT.X, elevationPT.Y, elevationPT.Z+100)
		ln = Line.CreateBound(elevationPT, elptRotate)

		#Calculate the angle between Model Mid Point and Elevation Point.
		elevationPTY = elevationPT.Y
		elevationPTX = elevationPT.X							
		combY = elevationPTY-modelMPY
		combX = elevationPTX-modelMPX			
		ang = atan2(combY, combX)

		#Create elevation marker and elevation in position 0.
		eleMarker = ElevationMarker.CreateElevationMarker(doc, viewType.Id, elevationPT, 100)
		ele = eleMarker.CreateElevation(doc, doc.ActiveView.Id , 0)
		
		#Rotate elevation marker towars model line.
		ElementTransformUtils.RotateElement(doc, eleMarker.Id, ln, ang)
		
		#	
		crManager = ele.GetCropRegionShapeManager()
		#crShape = crManager.GetCropRegionShape()

		newCurveLoop = []
		newCurveLoop.Add(l1)
		newCurveLoop.Add(l2)
		newCurveLoop.Add(l3)
		newCurveLoop.Add(l4)
			
		cLoop = CurveLoop.Create(newCurveLoop)

		try:			
			crManager.SetCropRegionShape(cLoop)
			lst.append("Elevation Created")
		
		except:
			pass
			lst.append("Missed Elevation")

	TransactionManager.Instance.TransactionTaskDone()
	
	OUT = lst
	
else:

	OUT = "Set toggle to TRUE"