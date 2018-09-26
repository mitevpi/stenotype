using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stenotype
{
    public class FamilySTX : FamilyST
    {
        public ObjectId Id { get; set; }

        public long FamilyFileSize { get; set; }
        public int PlacedInstancesCount { get; set; }

        public FamilySTX(Family family) : base(family)
        {
            FamilyFileSize = GetFamilyFileSize();
            PlacedInstancesCount = GetFamilyInstances().Count;
            Serialized = JsonConvert.SerializeObject(this);
            JsonObject = JObject.Parse(Serialized);
        }
    }
}