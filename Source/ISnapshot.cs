namespace MaslasBros.Snapshoting
{
    public interface ISnapshot
    {
        ///<summary>The SMRI of this instance</summary>
        public uint SMRI { get; }

        /// <summary>
        /// Registers the loaded snapshot and model to the snapshots and models cache
        /// </summary>
        /// <param name="sMRI">The loaded SMRI</param>
        /// <param name="snapshot">The reference to the model instance</param>
        /// <param name="model">The loaded snapshot model</param>
        public void LoadSnapshot(uint sMRI, ISnapshotModel model);

        /// <summary>
        /// Call after registering the ISnapshot to the manager to make it retrieve the references SMRIs of it.
        /// <para>Primarily used in loading</para>
        /// </summary>
        public void RetrieveReferences();

        ///<summary>Call to update the ISnapshotModel associated with this ISnapshot instance through the Snapshot Manager.</summary>
        public void UpdateToManager();
    }
}