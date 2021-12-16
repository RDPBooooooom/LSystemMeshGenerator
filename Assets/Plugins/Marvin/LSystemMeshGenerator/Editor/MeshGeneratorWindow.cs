#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Marvin.LSystemMeshGenerator
{
	/// <summary>
	/// Editor Window to create a mesh with the MeshGenerator
	/// </summary>
	public class MeshGeneratorWindow : EditorWindow
	{

		#region Variables
		public static MeshGeneratorWindow instance;

		private MeshGenerator meshGenerator;

		private LSystem lSystem;

		private Material mat;

		private int iterations = 1;
		private float distancePerStep = 1;
		private float anglePerStep = 45;
		private float startDiameter = 1;
		private float endDiameter = 1;

		private GUIStyle titleStyle;

		private Mesh latestMesh;

		private Vector2 scrollPosition;
		#endregion

		#region Unity Methods
		protected void OnGUI()
		{
			ShowInspector();
			ShowButtons();
		}

		protected void OnEnable()
		{
			InitStyles();
			meshGenerator = new MeshGenerator();
		}
		#endregion

		#region Draw UI
		/// <summary>
		/// Draws all Inputfields in the EditorWindow
		/// </summary>
		protected virtual void ShowInspector()
		{
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
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
					message += "\n\nCharacters not mentioned above aren't triggering any action. You can use them as placeholders to generate more characters.";

					EditorGUILayout.Space();
					EditorGUILayout.HelpBox(message, MessageType.Info);
				}
			}

			// Configuration
			EditorGUILayout.BeginVertical("box");
			ShowTitle("Configuration");

			iterations = EditorGUILayout.IntSlider(new GUIContent("Iterations", "Number of iterations the L System goes trough. \nMin: 0 \nMax: 1000"), iterations, 1, 1000);
			distancePerStep = EditorGUILayout.FloatField(new GUIContent("Distance per step", $"The distance that is created when an \"Up Action\" in the L System is executed. \nMin: 0.00001f \nMax: {float.MaxValue}"), Mathf.Clamp(distancePerStep, 0.00001f, float.MaxValue));
			anglePerStep = EditorGUILayout.FloatField(new GUIContent("Angle per step", "The angle change that happens when an \"Angle Action\" in the L System is executed. \nMin: -359.999f \nMax: 359.999f"), Mathf.Clamp(anglePerStep, -359.999f, 359.999f));
			startDiameter = EditorGUILayout.FloatField(new GUIContent("Start diameter", $"The diameter at the start of the mesh. \nMin: 0.00001f \nMax: {float.MaxValue}"), Mathf.Clamp(startDiameter, 0.00001f, float.MaxValue));
			endDiameter = EditorGUILayout.FloatField(new GUIContent("End diameter", $"The diameter at a endpoint of a mesh. \nMin: 0.00001f \nMax: {float.MaxValue}"), Mathf.Clamp(endDiameter, 0.00001f, float.MaxValue));

			mat = (Material)EditorGUILayout.ObjectField(new GUIContent("Material", "The material that is applied to the mesh"), mat, typeof(Material), false);

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
		}

		/// <summary>
		/// Draws a title in the EditorWindow
		/// </summary>
		/// <param name="title">Text to display</param>
		protected void ShowTitle(string title)
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField(title, titleStyle);
			EditorGUILayout.Space();
		}

		/// <summary>
		/// Draws all buttons in the EditorWindow
		/// </summary>
		protected virtual void ShowButtons()
		{
			bool generatePressed = GUILayout.Button("Generate mesh");

			bool savePressed = GUILayout.Button("Save mesh");

			if (generatePressed)
			{

				if (ValidateGeneration())
				{
					if (CheckBigValues())
					{
						if (!EditorUtility.DisplayDialog("You're using Big values", "You're using big values. Dependig on your rules and seed the generation can take very long! \n Please note that there will be no way to stop this operation without closing Unity. Make sure you have saved the changes that you want to keep before generating this mesh!", "I'm sure", "Cancel")) return;
					}

					latestMesh = meshGenerator.GenerateMesh(lSystem, iterations, distancePerStep, anglePerStep, startDiameter, endDiameter);

					GameObject gameObject = new GameObject();
					gameObject.AddComponent<MeshRenderer>().material = mat;
					gameObject.AddComponent<MeshFilter>().mesh = latestMesh;
				}
			}

			if (savePressed)
			{
				if (latestMesh != null)
				{

					string path = EditorUtility.SaveFilePanelInProject(
					"Save mesh",
					"New mesh",
					"asset",
					"Choose where to save the mesh.");

					if (path != "")
					{
						EditorUtils.SaveAsset<Mesh>(latestMesh, path);
					}
				}
				else
				{
					EditorUtility.DisplayDialog("No mesh to save", "There is no mesh to save. Please generate a mesh first!", "Ok");
				}
			}
		}

		/// <summary>
		/// Validates if all important parameter for the generation were set. 
		/// </summary>
		/// <returns>True if all validations were successful</returns>
		protected virtual bool ValidateGeneration()
		{
			if (lSystem == null)
			{
				EditorUtility.DisplayDialog("No LSystem", "There is no LSystem assigned! Please assigne one!", "Ok");
				return false;
			}
			if (lSystem.RuleSet == null)
			{
				EditorUtility.DisplayDialog("No LSystem Rule Set", "There is no LSystem Rule Set assigned! Please assigne one!", "Ok");
				return false;
			}
			return true;
		}

		/// <summary>
		/// Opens a new MeshGeneratorEditor Window
		/// </summary>
		public static void ShowMeshGenerator()
		{
			instance = EditorWindow.GetWindow<MeshGeneratorWindow>();
			instance.titleContent = new GUIContent("Mesh Generator");
		}
		#endregion

		/// <summary>
		/// Check for big numbers in the input fields
		/// </summary>
		/// <returns>true if big values are used</returns>
		protected virtual bool CheckBigValues()
		{
			if (iterations > 5) return true;
			if (distancePerStep > 5) return true;
			if (startDiameter > 4) return true;
			if (endDiameter > 4) return true;

			return false;
		}

		/// <summary>
		/// Initializes the Styles used in the EditorWindow
		/// </summary>
		protected virtual void InitStyles()
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
#endif