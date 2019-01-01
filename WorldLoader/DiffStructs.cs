using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WorldLoader
{
    public struct DiffFile
    {
        public uint magic;
        public int version;
        public string unityCompiledVersion;
        public List<GameObjectChange> changes;
        public List<GameObjectAdd> adds;
        public List<GameObjectRemove> removes;
        public void Read(BinaryReader r)
        {
            magic = r.ReadUInt32();
            version = r.ReadInt32();
            unityCompiledVersion = r.ReadString();
            changes = new List<GameObjectChange>();
            int changesCount = r.ReadInt32();
            for (int i = 0; i < changesCount; i++)
            {
                GameObjectChange change = new GameObjectChange();
                change.Read(r);
                changes.Add(change);
            }
            adds = new List<GameObjectAdd>();
            int addsCount = r.ReadInt32();
            for (int i = 0; i < addsCount; i++)
            {
                GameObjectAdd add = new GameObjectAdd();
                add.Read(r);
                adds.Add(add);
            }
            removes = new List<GameObjectRemove>();
            int removesCount = r.ReadInt32();
            for (int i = 0; i < removesCount; i++)
            {
                GameObjectRemove remove = new GameObjectRemove();
                remove.Read(r);
                removes.Add(remove);
            }
        }
    }

    public struct GameObjectChange
    {
        public long pathId;
        public List<ComponentChangeOrAdd> changes;
        public List<ComponentRemove> removes;
        public void Read(BinaryReader r)
        {
            pathId = r.ReadInt64();
            changes = new List<ComponentChangeOrAdd>();
            int changesCount = r.ReadInt32();
            for (int i = 0; i < changesCount; i++)
            {
                ComponentChangeOrAdd change = new ComponentChangeOrAdd();
                change.Read(r);
                changes.Add(change);
            }
            int removesCount = r.ReadInt32();
            for (int i = 0; i < removesCount; i++)
            {
                ComponentRemove remove = new ComponentRemove();
                remove.Read(r);
                removes.Add(remove);
            }
        }
    }

    public struct ComponentChangeOrAdd
    {
        public bool isNewComponent;
        public int componentIndex;
        public string componentType;
        public List<FieldChange> changes;
        public void Read(BinaryReader r)
        {
            isNewComponent = r.ReadBoolean();
            componentIndex = r.ReadInt32();
            componentType = r.ReadString();
            changes = new List<FieldChange>();
            int changesCount = r.ReadInt32();
            for (int i = 0; i < changesCount; i++)
            {
                FieldChange change = new FieldChange();
                change.Read(componentType, r);
                changes.Add(change);
            }
        }
    }

    public struct ComponentRemove
    {
        public int componentIndex;
        public void Read(BinaryReader r)
        {
            componentIndex = r.ReadInt32();
        }
    }

    public struct FieldChange
    {
        public string fieldName;
        public string fieldType;
        public byte[] data;
        public void Read(string typeName, BinaryReader r)
        {
            UnityEngine.Debug.Log("HKWE FC " + fieldName + " + " + fieldType);
            fieldName = r.ReadString();
            fieldType = r.ReadString();
            data = r.ReadBytes(r.ReadInt32());
        }
    }

    public struct GameObjectAdd
    {
        public long pathId;
        public long parentId;
        public void Read(BinaryReader r)
        {
            pathId = r.ReadInt64();
            parentId = r.ReadInt64();
        }
    }

    public struct GameObjectRemove
    {
        public long pathId;
        public void Read(BinaryReader r)
        {
            pathId = r.ReadInt64();
        }
    }
}
