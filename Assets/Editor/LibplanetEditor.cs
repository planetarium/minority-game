using System.IO;
using System.Reflection;
using LibplanetUnity;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class LibplanetEditor
    {
        [MenuItem("Tools/Libplanet/Delete All")]
        public static void DeleteAllEditor()
        {
            FieldInfo pathField =
                typeof(Agent).GetField("DefaultStoragePath", BindingFlags.Static | BindingFlags.NonPublic);
            var storagePath = (string) pathField.GetValue(null);
            Debug.Log(storagePath);
            Directory.Delete(storagePath, true);
        }
    }
}
