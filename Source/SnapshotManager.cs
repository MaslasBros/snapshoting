using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MaslasBros.Snapshoting
{
    public abstract class SnapshotManager
    {
        ///<summary>Internal SMRI counter</summary>
        private static uint smriInternal = 0;
        ///<summary>Get the current SMRI number</summary>
        protected static uint sMRI => smriInternal;

        ///<summary>Dictionary storing the ISnapshots. Accessible with their unique SMRIs.</summary>
        Dictionary<uint, ISnapshot> snapshots = new Dictionary<uint, ISnapshot>();
        ///<summary>Dictionary storing the ISnapshotModels. Accessible with their unique SMRIs.</summary>
        Dictionary<uint, ISnapshotModel> models = new Dictionary<uint, ISnapshotModel>();

        public IReadOnlyDictionary<uint, ISnapshot> Snapshots => snapshots;
        public IReadOnlyDictionary<uint, ISnapshotModel> Models => models;

        ///<summary>Subscribe to this event to get notified when it's time to take a snapshot.</summary>
        protected event Action onTakeSnapshot;
        ///<summary>Raises the onTakeSnapshot event, primarily for external use.</summary>
        public void OnTakeSnapshot()
        {
            if (onTakeSnapshot != null)
            {
                onTakeSnapshot();
            }
        }

        ///<summary>Subscribe to this event to get notified when snapshoting is completed</summary>
        public event Action onSnapshotCompeted;
        ///<summary>Raises the onSnapshotCompeted event, primarily for internal use.</summary>
        protected virtual void OnSnapshotCompeted()
        {
            if (onSnapshotCompeted != null)
            {
                onSnapshotCompeted();
            }
        }

        ///<summary>Increments and returns an SMRI for use on a new ISnapshot instance.</summary>
        public virtual uint GetCurrentSMRI() => ++smriInternal;

        #region SNAPSHOT_REGISTRATION
        /// <summary>
        /// Call to add the passed ISnapshot to the snapshots cache.
        /// </summary>
        /// <exception cref = "System.ArgumentException">Passed sMRI is a duplicate</exception>
        public virtual void AddToSnapshots(uint sMRI, ISnapshot snapshot)
        {
            if (!snapshots.TryAdd(sMRI, snapshot))
            { throw new ArgumentException("Passed sMRI is a duplicate."); }
        }

        ///<summary>Constructs and adds an ISnapshotModel struct instance to the models cache.</summary>
        public virtual void ConstructAddSnapshotModel<T>(uint sMRI) where T : ISnapshotModel, new()
        {
            T instance = (T)Activator.CreateInstance(typeof(T));
            instance.refSMRIs = new List<uint>();

            AddToModels(sMRI, (ISnapshotModel)instance);
        }

        /// <summary>
        /// Call to add the passed ISnapshotModel to the models cache.
        /// </summary>
        /// <exception cref = "System.ArgumentException">Passed sMRI is a duplicate</exception>
        public virtual void AddToModels(uint sMRI, ISnapshotModel model)
        {
            if (!models.TryAdd(sMRI, model))
            { throw new ArgumentException("Passed sMRI is a duplicate."); }

            smriInternal = sMRI;
        }
        #endregion

        #region SNAPSHOT_UPDATING
        /// <summary>
        /// Returns the ISnapshotModel associated with the passed sMRI from the models cache.
        /// </summary>
        /// <exception cref = "System.ArgumentException">Passed SMRI not present in model dictionary.</exception>
        public T AccessModel<T>(uint sMRI)
        {
            if (models.ContainsKey(sMRI))
            {
                return (T)models[sMRI];
            }
            else { throw new ArgumentException("Passed SMRI not present in model dictionary. SMRI: " + sMRI); }
        }

        ///<summary>Updates the values of the ISnapshotModel associated with the passed sMRI, with the values of the passed ISnapshotModel.</summary>
        /// <exception cref="ArgumentException">Passed SMRI not present in model dictionary.</exception>
        public virtual void WriteToModel(uint sMRI, ISnapshotModel model)
        {
            if (models.ContainsKey(sMRI))
            {
                models[sMRI] = model;
            }
            else { throw new ArgumentException("Passed SMRI not present in model dictionary."); }
        }

        ///<summary>
        /// Begins the data updating and data serialization of every ISnapshotModel present in the cache dictionary
        /// on a new thread.
        /// </summary>
        protected void SnapshotProcess(string finalSaveFolder, string finalSaveName)
        {
            //Data gathering proccess
            foreach (KeyValuePair<uint, ISnapshot> snapshotCandidate in snapshots)
            {
                snapshotCandidate.Value.UpdateToManager();
            }

            Task.Run(() =>
            {
                if (!Directory.Exists(finalSaveFolder))
                { Directory.CreateDirectory(finalSaveFolder); }

                //Model Grouping process
                Dictionary<Type, List<object>> groups = new Dictionary<Type, List<object>>();
                foreach (KeyValuePair<uint, ISnapshotModel> modelCandidate in models)
                {
                    Type modeltype = modelCandidate.Value.GetType();
                    if (!groups.ContainsKey(modeltype))
                    {
                        groups[modeltype] = new List<object>();
                        groups[modeltype].Add(modelCandidate.Value);
                    }
                    else
                    {
                        groups[modeltype].Add(modelCandidate.Value as object);
                    }
                }

                //Finaly, model serialization
                byte[] serGroups = MessagePackSerializer.Serialize(groups);
                string finalFile = Path.Combine(finalSaveFolder, finalSaveName);

                if (File.Exists(finalFile))
                { File.Delete(finalFile); }

                File.WriteAllBytes(finalFile, serGroups);

            }).ContinueWith((t) =>
            {
                OnSnapshotCompeted();
            });
        }

        ///<summary>Removes the ISnapshot & ISnapshotModel references from the manager cache based on the passed sMRI.</summary>
        public virtual void RemoveFromManager(uint sMRI)
        {
            snapshots.Remove(sMRI);
            models.Remove(sMRI);
        }
        #endregion
    }
}