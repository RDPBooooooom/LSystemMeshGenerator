#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Marvin.LSystemMeshGenerator
{
	/// <summary>
	/// Contains all available Menus for the LSystemMeshGenerator tool.
	/// </summary>
	public static class MenuItems
	{

		/// <summary>
		/// Menu to create and save a new LSystemRuleSet asset.
		/// </summary>
		[MenuItem(Constants.ToolMenuPath + "New L System Rule Set")]
		private static void NewLSystemRuleSet()
		{
			string path = EditorUtility.SaveFilePanelInProject(
				"New Rule Set",
				"L System Rule Set",
				"asset",
				"Define the name for the L System Rule Set asset");

			if (path != "")
			{
				EditorUtils.CreateAsset<LSystemRuleSet>(path);
			}
		}

		/// <summary>
		/// Menu to create and save a new LSystem asset.
		/// </summary>
		[MenuItem(Constants.ToolMenuPath + "New L System")]
		private static void NewLSystem()
		{
			string path = EditorUtility.SaveFilePanelInProject(
				"New L System",
				"L System",
				"asset",
				"Define the name for the L System asset");

			if (path != "")
			{
				EditorUtils.CreateAsset<LSystem>(path);
			}
		}

		/// <summary>
		/// Creates a MeshGeneratorWindow
		/// </summary>
		[MenuItem(Constants.ToolMenuPath + "Show Mesh Generator")]
		private static void ShowMeshGenerator()
		{
			MeshGeneratorWindow.ShowMeshGenerator();
		}
	}
}
#endif