namespace MaslasBros.Snapshoting
{
    public interface ISnapshot
    {
        ///<summary>The SMRI of THIS instance</summary>
        public uint sMRI { get; }

        ///<summary>
        /// Call upon construction to subscribe THIS instance to the Snapshot Manager
        /// <para>Primarily used in saving</para>
        /// </summary>
        public uint RegisterToManager();

        ///<summary>
        /// Call upon construction to subscribe THIS instance to the Snapshot Manager with a deserialized SMRI and ISnapshotModel.
        /// <para>Primarily used in loading</para>
        /// </summary>
        public uint RegisterToManager(uint loadedSMRI, ISnapshotModel model);

        ///<summary>Call to update the ISnapshotModel associated with THIS ISnapshot instance through the Snapshot Manager.</summary>
        public void UpdateToManager();

        ///<summary>Removes THIS instance from the Snapshot Manager cache and events.</summary>
        public void RemoveFromManager();
    }
}