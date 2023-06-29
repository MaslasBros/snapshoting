using System;

namespace MaslasBros.Snapshoting
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class SnapshotSerializationOrder : Attribute
    {
        /// <summary>The serialization order of the attributed class</summary>
        uint serOrder;

        /// <summary>The serialization order of the attributed class</summary>
        public uint SerOrder => serOrder;

        private SnapshotSerializationOrder() { }

        /// <summary>The serialization order of the attributed class</summary>
        public SnapshotSerializationOrder(uint serOrder) : this()
        {this.serOrder = serOrder;}
    }
}
