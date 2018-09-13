using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stenotype
{
    /// <summary>
    /// Wrapper class for a Revit Room object.
    /// </summary>
    /// <remarks>
    /// ADD.
    /// </remarks>
    public class RoomST
    {
        /// <summary>
        /// The document to which the Room belongs to.
        /// </summary>
        [NonSerialized()] public readonly Document Doc;
        [NonSerialized()] private readonly Application _app;

        /// <summary>
        /// An Option Object for working with geometry associated with the Room.
        /// </summary>
        [NonSerialized()] public readonly Options GeomOption;

        /// <summary>
        /// Edges of the Room as an array.
        /// </summary>
        [NonSerialized()] public readonly EdgeArray EdgeArray;

        [NonSerialized()] public readonly Room Room;
        [JsonProperty()] private string RoomName { get => Room.Name.ToString(); }

        /// <summary>
        /// A class for working with the Revit Room object.
        /// </summary>
        /// <param name="room">A Revit Room object.</param>
        public RoomST(Room room)
        {
            Room = room;
            Doc = room.Document;
            _app = Doc.Application;
            GeomOption = CreateGeometryOption();
            EdgeArray = GetRoomEdges();
        }

        /// <summary>
        /// Create a Revit Geometry Option for working with/extracting geometry from native Revit elements.
        /// </summary>
        /// <returns>A Geometry Option object.</returns>
        public Options CreateGeometryOption()
        {
            Options geomOption = _app.Create.NewGeometryOptions();
            geomOption.DetailLevel = ViewDetailLevel.Undefined;
            geomOption.IncludeNonVisibleObjects = false;
            return geomOption;
        }

        /// <summary>
        /// Get Room Edges.
        /// </summary>
        /// <returns>An Edge Array object.</returns>
        public EdgeArray GetRoomEdges()
        {
            GeometryElement geometryElement = Room.get_Geometry(GeomOption);
            EdgeArray edgeArray = geometryElement.Select(g => (Solid)g).Select(s => s.Edges) as EdgeArray;

            return edgeArray;
        }

        /// <summary>
        /// Create a solid representation of the room's volume in the model via a DirectShape.
        /// </summary>
        public void GetRoomMass()
        {
            using (Transaction t = new Transaction(Doc, "Create Room Mass"))
            {
                t.Start();

                GeometryElement geometryElement = Room.get_Geometry(GeomOption);
                foreach (GeometryObject geoObj in geometryElement)
                {
                    Solid solid = geoObj as Solid;
                    DirectShape ds = DirectShape.CreateElement(Doc, new ElementId(BuiltInCategory.OST_GenericModel));
                    ds.SetShape(new GeometryObject[] { solid });
                }

                t.Commit();
                t.Dispose();
            }
        }
    }
}
