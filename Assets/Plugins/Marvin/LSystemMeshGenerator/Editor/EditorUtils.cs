#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Marvin.LSystemMeshGenerator
{
	/// <summary>
	/// Utils for commonly used methods in the Editor
	/// </summary>
	public static class EditorUtils
	{
		/// <summary>
		/// Creates and saves a ScriptableObject
		/// </summary>
		/// <typeparam name="T">Scriptable Object to create</typeparam>
		/// <param name="path">The path where the ScriptableObject is created</param>
		/// <returns>The created ScriptableObject</returns>
		public static T CreateAsset<T>(string path) where T : ScriptableObject
		{
			T dataClass = (T)ScriptableObject.CreateInstance<T>();
			AssetDatabase.CreateAsset(dataClass, path);
			AssetDatabase.Refresh();
			AssetDatabase.SaveAssets();
			return dataClass;
		}

		/// <summary>
		/// Saves <paramref name="asset"/> in the given <paramref name="path"/>
		/// </summary>
		/// <typeparam name="T">Class to save</typeparam>
		/// <param name="asset">Asset to save</param>
		/// <param name="path">Directiory to save the asset in</param>
		public static void SaveAsset<T>(T asset, string path) where T : Object
		{
			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.Refresh();
			AssetDatabase.SaveAssets();
		}
	}
}
#endif