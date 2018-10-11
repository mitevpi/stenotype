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
    /// <summary>
    /// Extension of the base FamilyST class for serialization/data export. May include performance-intensive methods.</summary>
    /// <remarks>
    /// TODO.
    /// </remarks>
    public class FamilySTX : FamilyST
    {
        /// <summary>
        /// ID parameter for MongoDB interaction.
        /// </summary>
        public ObjectId Id { get; set; }

        /// <summary>
        /// The file size of the Family in Bytes.
        /// </summary>
        public long FamilyFileSize { get; set; }

        /// <summary>
        /// The number of placed Families (instances).
        /// </summary>
        public int PlacedInstancesCount { get; set; }

        /// <summary>
        /// A class for working with Revit Family Elements.
        /// </summary>
        /// <param name="family">The Family object used to instantiate the class.</param>
        public FamilySTX(Family family) : base(family)
        {
            FamilyFileSize = GetFamilyFileSize();
            PlacedInstancesCount = GetFamilyInstances().Count;
            Serialized = JsonConvert.SerializeObject(this);
            JsonObject = JObject.Parse(Serialized);
        }
    }
}