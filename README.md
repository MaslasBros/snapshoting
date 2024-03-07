# Snapshoting
A C# library that provides the infrastructure to build tools capable of collecting and serializing instance data on runtime without interupting the main thread.

# Manual

For an object to be marked as save-able, the ISnapshot interface must be applied to the class declaration, e.g. 
```csharp
public class DummySnapshotClass : ISnapshot
{}
```

### ISnapshot Interface

1) The SMRI of the instance, this is assigned later on.
```csharp
///<summary>The SMRI of this instance</summary>
private uint smri = 0;
//..
//..
//..
  ///<summary>The SMRI of this instance</summary>
  public uint SMRI => smri;
```
2) The base registration of the instance to the snapshot pool, so it can be accessed later and be serialized.
```csharp
  public DummySnapshotClass()
  {
      smri = common.SaveManager.RegisterSnapshot<SClockManager>(this); 
  }
```
3) The method that will be externally OR internally called when the models gets loaded back into the instance after a save reload.
```csharp
  public void LoadSnapshot(uint sMRI, ISnapshotModel model)
  {
      //Instance setup like a fresh registration
      this.smri = sMRI;
      //Registers a loaded ISnapshot class and its model to the SaveManager
      common.SaveManager.RegisterLoadedSnapshot(this.smri, this, model);
  
      SDummySnapshotClass cModel = (SDummySnapshotClass)model;
      //Access to the de-serialized model information that can be written back 
      //to the class fields.
      this.intField = cModel.intField;
      this.intField2 = cModel.intField2;
      //etc...
  }
```
4) The *RetrieveReferences* method gets called at the end of the loading process so every ISnapshot class can setup its own way of retrieving the serialized references through their SMRIs.
```csharp
  public void RetrieveReferences()
  {
      //Iterate on the RefSMRIs list and retrieve any reference from the Snapshots cache at
      SaveManager.Snapshots[iterated_SMRI];
  }
```
5) When its time for any type of save or snapshot taking, the UpdateToManager method gets called on every ISnapshot class
```csharp
  public void UpdateToManager()
  {
      //Retrieve the created model of your class with your class's SMRI.
      SDummySnapshotClass temp = SaveManager.AccessModel<SDummySnapshotClass>(this.smri);
      temp.SnapshotSMRI = smri;
  
      //Update the fields
      temp.intField = this.intField;
      temp.intField2 = this.intField2;
  
      //The list must be cleared after every save so we don't duplicate the values inside the RefSmris list
      temp.RefSMRIs = new List<uint>();
      
      //Write the model back to the cache
      SaveManager.WriteToModel(this.smri, temp);
  }
```

After the above operation your class is ready to be serialized.

### ISnapshotModel interface
Every ISnapshot class must have a data courier class alongside it that implement the ISnapshotModel interface. e.g.
```csharp
///<summary>A class representing th ClockManager class instance save model.</summary>
[MessagePackObject]
public class SClockManager : ISnapshotModel
{}
```
The [SnapshotSerializationOrder(0, 1, 2, ...)] class attribute can be used on the model if you want to specify a serialization order for the model, if it's omitted then it will be serialized at the end of the list.
```csharp
///<summary>A class representing th ClockManager class instance save model.</summary>
[MessagePackObject, SnapshotSerializationOrder(0)]]
public class SDummySnapshotClass: ISnapshotModel
{}
```
For the serialization and de-serialization to happen correctly a specific setup must be used throughout the model.
1) Default constructor, used from the initial creation of the model
```csharp
public SDummySnapshotClass() { }
``` 

2.a) Constructor accepting an object[], used for the de-serialization process
```csharp
public SDummySnapshotClass(object[] args)
{}
```
2.b) The overriding constructor must always have at least these two conversions in it.
```csharp
  public SDummySnapshotClass(object[] args)
  {
      //MANDATORY===============================================================================
      //Convert from object to uint32 type, essentially the saved SMRI of the ISnapshot instance
      this.SnapshotSMRI = Convert.ToUInt32(args[0]);
  
      //Populate the RefSMRIs list with the saved SMRIs from the de-serialized object array
      this.RefSMRIs = new List<uint>();
      for (int i = 0; i < (args[1] as object[]).Length; i++)
      {
          RefSMRIs.Add(Convert.ToUInt32((args[1] as object[])[i]));
      }
      //========================================================================================
  
      //Custom conversions based on the saved fields - Pay attention to the index access of the object[]
      this.intField = (int)args[2];
      this.intField2 = (int)args[3];
   }
```

3) Two mandatory properties must be inherited too.
```csharp
  //MANDATORY==============================
  [Key(0)]
  public uint SnapshotSMRI { get; set; }
  [Key(1)]
  public List<uint> RefSMRIs { get; set; }
  //=======================================
  
  //Your own fields
  [Key(2)]
  public int intField;
  [Key(3)]
  public int intField2;
```
###### The [Key(0)] attribute specifies the order of serialization of the fields as shown in [Message Pack C#](https://github.com/neuecc/MessagePack-CSharp#quick-start)
#### What's inside the object[]
When de-serialized the object[] contains the fields marked with the [Key(X)] in the order they are set to be serialized in the ISnapshotModel class. e.g. the above model will be serialized as
```json
{
  "SDummySnapshotClass, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null": [
    [
      1, //The SMRI - object[0]
      [], //The RefSMRIs list - object[1]
      2, //intField - object[2]
      456, //intField2 - object[3]
    ]
  ]
}
```
so in order for the model to be instantiated back with its de-serialized information the manual allocation of each variable is needed. (Casting back to ISnapshotModel won't work because the information gets deserialized back to an object[], trust me I've lost sleep from it).

# Dependencies

- [MessagePack-CSharp](https://github.com/MessagePack-CSharp/MessagePack-CSharp): Extremely Fast MessagePack Serializer for C#(.NET, .NET Core, Unity, Xamarin). / msgpack.org[C#]
