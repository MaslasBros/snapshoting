using System;

namespace MaslasBros.Snapshoting
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class SnapshotSerializeOrder : Attribute
    {
        /// <summary>The serialization order of the attributed class</summary>
        uint serOrder;

        /// <summary>The serialization order of the attributed class</summary>
        public uint SerOrder => serOrder;

        private SnapshotSerializeOrder() { }

        /// <summary>The serialization order of the attributed class</summary>
        public SnapshotSerializeOrder(uint serOrder) : this()
        {
            this.serOrder = serOrder;
        }
    }
}
