#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Marvin.LSystemMeshGenerator
{
	public static class EditorUtils
	{
		public static T CreateAsset<T>(string path) where T : ScriptableObject
		{
			T dataClass = (T)ScriptableObject.CreateInstance<T>();
			AssetDatabase.CreateAsset(dataClass, path);
			AssetDatabase.Refresh();
			AssetDatabase.SaveAssets();
			return dataClass;
		}

		public static void SaveAsset<T>(T asset, string path) where T : Object
		{
			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.Refresh();
			AssetDatabase.SaveAssets();
		}

	}
}
#endif