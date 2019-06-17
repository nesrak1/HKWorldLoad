using AssetsTools.NET;
using AssetsTools.NET.Extra;
using BundleLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WorldLoader
{
    public class ComponentDiffReplacer
    {
        public static AssetsReplacer DiffComponent(AssetFileInfoEx info, AssetTypeValueField baseField, ClassDatabaseFile cldb, ComponentChangeOrAdd compChange, ulong pathId)
        {
            //todo: shouldn't need to overwrite, using info.curFileType would probably work fine
            int classId = AssetHelper.FindAssetClassByName(cldb, compChange.componentType).classId;

            foreach (FieldChange fieldChange in compChange.changes)
            {
                AssetTypeValueField field = baseField;
                foreach (string curPath in fieldChange.fieldName.Split('/'))
                {
                    field = field.Get(curPath);
                }

                object value = DecodeValueByName(fieldChange.data, fieldChange.fieldType);
                if (value != null)
                {
                    //UnityEngine.Debug.Log("HKWE SV " + value.ToString());
                    field.GetValue().Set(value);
                }
                else
                {
                    //UnityEngine.Debug.Log("HKWE NV");
                }
            }

            byte[] moveAsset;
            using (MemoryStream memStream = new MemoryStream())
            using (AssetsFileWriter writer = new AssetsFileWriter(memStream))
            {
                writer.bigEndian = false;
                baseField.Write(writer);
                moveAsset = memStream.ToArray();
            }

            return new AssetsReplacerFromMemory(0, pathId, classId, 0xFFFF, moveAsset);
        }
        private static object DecodeValueByName(byte[] data, string fieldType)
        {
            switch (fieldType)
            {
                case "bool":
                    return BitConverter.ToBoolean(data, 0);
                case "char":
                    return BitConverter.ToChar(data, 0);
                case "double":
                    return BitConverter.ToDouble(data, 0);
                case "short":
                    return BitConverter.ToInt16(data, 0);
                case "int":
                    return BitConverter.ToInt32(data, 0);
                case "long":
                    return BitConverter.ToInt64(data, 0);
                case "float":
                    return BitConverter.ToSingle(data, 0);
                case "ushort":
                    return BitConverter.ToUInt16(data, 0);
                case "uint":
                    return BitConverter.ToUInt32(data, 0);
                case "ulong":
                    return BitConverter.ToUInt64(data, 0);
                //todo
                case "byte":
                case "sbyte":
                    return data[0];
                default:
                    return null;
            }
        }
    }
}
