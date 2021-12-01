using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Marvin.LSystemMeshGenerator
{

	public class MeshGeneratorWindow : EditorWindow
	{

		#region Variables
		public static MeshGeneratorWindow instance;

		private LSystem lSystem;

		private Material mat;

		private int iterations = 1;
		private float distancePerStep = 1;
		private float anglePerStep = 45;
		private float startDiameter = 1;
		private float endDiameter = 1;

		private GUIStyle titleStyle;

		#endregion

		public static void ShowMeshGenerator()
		{
			instance = EditorWindow.GetWindow<MeshGeneratorWindow>();
			instance.titleContent = new GUIContent("Mesh Generator");
		}

		private void OnGUI()
		{

			ShowInspector();
			ShowButtons();
		}

		private void ShowInspector()
		{
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("L System data", titleStyle);
			EditorGUILayout.Space();

			// L System & Rule Set Inspector 

			lSystem = (LSystem)EditorGUILayout.ObjectField("L System", lSystem, typeof(LSystem), false);

			if (lSystem != null)
			{
				Editor.CreateEditor(lSystem).OnInspectorGUI();

				if (lSystem.RuleSet != null)
				{
					ShowTitle("L System Rule Set");
					Editor.CreateEditor(lSystem.RuleSet).OnInspectorGUI();

					//Show Info which chars are usable with the L System used by the EditorWindow

					string message = "Following characters trigger an action in the L-System:";
					message += "\nF -> Draw mesh one step further";
					message += "\n< -> The next draw action is rotated one angle step on the Vector3.up axis";
					message += "\n> -> The next draw action is rotated one inverted angle step on the Vector3.up axis";
					message += "\n+ -> The next draw action is rotated one angle step on the Vector3.forward axis";
					message += "\n- -> The next draw action is rotated one inverted angle step on the Vector3.forward axis";
					message += "\n[ -> Opens a branch. Stores position and rotation";
					message += "\n] -> Closes the latest branch. Return to the latest stored position and rotation";
					
					EditorGUILayout.Space();
					EditorGUILayout.HelpBox(message, MessageType.Info);
				}
			}

			// Configuration
			EditorGUILayout.BeginVertical("box");
			ShowTitle("Configuration");

			iterations = EditorGUILayout.IntField(new GUIContent("Iterations", "Number of iterations the L System goes trough. \nMin: 0 \nMax: 1000"), Mathf.Clamp(iterations, 1, 1000));
			distancePerStep = EditorGUILayout.FloatField(new GUIContent("Distance per step", $"The distance that is created when an \"Up Action\" in the L System is executed. \nMin: 0.00001f \nMax: {float.MaxValue}"), Mathf.Clamp(distancePerStep, 0.00001f, float.MaxValue));
			anglePerStep = EditorGUILayout.FloatField(new GUIContent("Angle per step", "The angle change that happens when an \"Angle Action\" in the L System is executed. \nMin: -359.999f \nMax: 359.999f"), Mathf.Clamp(anglePerStep, -359.999f, 359.999f));
			startDiameter = EditorGUILayout.FloatField(new GUIContent("Start diameter", $"The diameter at the start of the mesh. \nMin: 0.00001f \nMax: {float.MaxValue}"), Mathf.Clamp(startDiameter, 0.00001f, float.MaxValue));
			endDiameter = EditorGUILayout.FloatField(new GUIContent("End diameter", $"The diameter at a endpoint of a mesh. \nMin: 0.00001f \nMax: {float.MaxValue}"), Mathf.Clamp(endDiameter, 0.00001f, float.MaxValue));

			mat = (Material)EditorGUILayout.ObjectField(new GUIContent("Material", "The material that is applied to the mesh"), mat, typeof(Material), false);

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndVertical();
		}

		private void ShowTitle(string title)
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField(title, titleStyle);
			EditorGUILayout.Space();
		}

		private void ShowButtons()
		{
			bool generatePressed = GUILayout.Button("Generate mesh");

			bool savePressed = GUILayout.Button("Save mesh");

			if (generatePressed)
			{
				Mesh mesh = MeshGenerator.GenerateMesh(lSystem, iterations, distancePerStep, anglePerStep, startDiameter, endDiameter);

				GameObject gameObject = new GameObject();
				gameObject.AddComponent<MeshRenderer>().material = mat;
				gameObject.AddComponent<MeshFilter>().mesh = mesh;
			}

		}

		private void OnEnable()
		{
			InitStyles();
		}

		private void InitStyles()
		{
			titleStyle = new GUIStyle();
			titleStyle.alignment = TextAnchor.MiddleCenter;
			titleStyle.fontSize = 16;

			if (EditorGUIUtility.isProSkin)
			{
				titleStyle.normal.textColor = Color.white;
			}
			else
			{
				titleStyle.normal.textColor = Color.black;
			}
		}
	}

}
