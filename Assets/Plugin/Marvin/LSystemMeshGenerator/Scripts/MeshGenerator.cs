using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Marvin.LSystemMeshGenerator
{
	public static class MeshGenerator
	{
		#region Variables

		private static Mesh mesh;

		private static float distancePerStep;

		private static float anglePerStep;

		private static float startDiameter;

		private static float endDiameter;

		private static Vector3 currentDirection;

		private static Stack<BranchInfo> branchStack;

		private static BranchInfo currentBranchInfo;

		#endregion

		#region Enums

		#endregion

		#region Mesh Generation

		public static Mesh GenerateMesh(LSystem lSystem, int iterations)
		{
			return GenerateMesh(lSystem, iterations, 1, 45, 1, 1);
		}

		public static Mesh GenerateMesh(LSystem lSystem, int iterations, float distancePerStep, float anglePerStep, float startDiameter, float endDiameter)
		{
			// Setup
			branchStack = new Stack<BranchInfo>();
			currentBranchInfo = null;

			MeshGenerator.distancePerStep = distancePerStep;
			MeshGenerator.anglePerStep = anglePerStep;
			MeshGenerator.startDiameter = startDiameter;
			MeshGenerator.endDiameter = endDiameter;

			currentDirection = new Vector3(1, 1, 0);

			string source = lSystem.GetLSystemString(iterations);

			Debug.Log(source);

			mesh = GetStartingMesh();

			foreach (char key in source)
			{
				//Debug.Log("Action for Key: " + key);
				switch (char.ToUpper(key))
				{
					case 'F':
						UpAction();
						break;
					case '[':
						// Branch out -> Save Pos
						StartBranchAction();
						break;
					case ']':
						// End Branch -> Get back to saved Pos
						EndBranchAction();
						break;
					case '<':
						AngleUpdateAction(true, Vector3.up);
						break;
					case '>':
						// AngleStep to right
						AngleUpdateAction(false, Vector3.up);
						break;
					case '+':
						// AngleStep forward
						AngleUpdateAction(true, Vector3.forward);
						break;
					case '-':
						// AngleStep backward
						AngleUpdateAction(false, Vector3.forward);
						break;
					default:
						Debug.LogWarning("illegal L System Operation: " + key);
						break;
				}
			}

			CloseMesh();

			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.RecalculateTangents();
			mesh.Optimize();

			branchStack.Clear();

			return mesh;
		}


		#region L System Actions

		private static void UpAction()
		{

			Vector3[] vertices = GetVertices();
			int[] wallTriangels = GetTriangles();

			mesh.vertices = mesh.vertices.Concat(vertices).ToArray();
			mesh.triangles = mesh.triangles.Concat(wallTriangels).ToArray();

			currentBranchInfo = null;
		}

		private static void AngleUpdateAction(bool isPositiv, Vector3 axis)
		{
			if (isPositiv)
			{
				currentDirection = Quaternion.AngleAxis(anglePerStep, axis) * currentDirection;
			}
			else
			{
				currentDirection = Quaternion.AngleAxis(-anglePerStep, axis) * currentDirection;
			}
		}

		private static void StartBranchAction()
		{
			BranchInfo currentInfo = new BranchInfo
			{
				currentDirection = currentDirection,
				currentVertexCount = mesh.vertexCount,
				lastVertices = GetLastVerticesFromMesh(4)
			};

			branchStack.Push(currentInfo);
		}

		private static void EndBranchAction()
		{
			currentBranchInfo = branchStack.Pop();

			MeshGenerator.currentDirection = currentBranchInfo.currentDirection;

			CloseMesh();
		}

		#endregion

		#region Mesh gen. helper methods

		private static Mesh GetStartingMesh()
		{
			Mesh startMesh = new Mesh();

			startMesh.vertices = new Vector3[]
				{
					new Vector3(0, 0, 0),
					new Vector3(1, 0, 0),
					new Vector3(1, 0, 1),
					new Vector3(0, 0, 1),
				};

			startMesh.triangles = new int[]
				{
					0, 1, 2,
					0, 2, 3
				};

			return startMesh;
		}

		private static Vector3[] GetVertices()
		{
			Vector3[] lastVertices;
			if (currentBranchInfo != null)
			{
				lastVertices = currentBranchInfo.lastVertices;
			}
			else
			{
				lastVertices = GetLastVerticesFromMesh(4);
			}

			for (int i = 0; i < lastVertices.Length; i++)
			{
				lastVertices[i] = GetForwardWithDirection(lastVertices[i], i);
			}

			return lastVertices;
		}

		private static Vector3[] GetLastVerticesFromMesh(int count)
		{
			Vector3[] lastVertices = new Vector3[count];
			Vector3[] current = mesh.vertices;

			int iteration = 0;

			for (int i = current.Length - count; iteration < count; iteration++)
			{
				lastVertices[iteration] = current[i];
				i++;
			}

			return lastVertices;
		}

		private static Vector3 GetForwardWithDirection(Vector3 toChange, int currentIndex)
		{
			return GetDirectionDistancePerStep(currentIndex) + toChange;
		}

		private static Vector3 GetDirectionDistancePerStep(int currentIndex)
		{
			switch (currentIndex)
			{
				case 0:
					return currentDirection.normalized * distancePerStep;
				case 1:
					return currentDirection.normalized * distancePerStep;
				case 2:
					return currentDirection.normalized * distancePerStep;
				case 3:
					return currentDirection.normalized * distancePerStep;
				default:
					return currentDirection.normalized * distancePerStep;
			}
		}

		private static int[] GetTriangles()
		{
			int verticeCount = mesh.vertexCount - 1;

			int[] triangles;

			/*
			*	Bottom Left Normal: -3
			*	Bottom Left Branch: 0
			*	Top Left Normal: -2
			*	Top Left Branch: 1
			*	Top Right Normal: -1
			*	Top Right Branch: 2
			*	Bottom Right Normal: 0
			*	Bottom Right Branch: 3
			*/
			if (currentBranchInfo != null)
			{
				int branchVertexCount = currentBranchInfo.currentVertexCount - 1;

				if (currentDirection.y >= 0)
				{
					triangles = new int[]
					{
						// Front
						branchVertexCount - 2, verticeCount + 3, branchVertexCount - 1,
						branchVertexCount - 2, verticeCount + 2, verticeCount + 3,
						// Left
						branchVertexCount - 2, branchVertexCount - 3, verticeCount + 1,
						branchVertexCount - 2, verticeCount + 1, verticeCount + 2,
						// Back
						branchVertexCount - 3, branchVertexCount, verticeCount + 4,
						branchVertexCount - 3, verticeCount + 4, verticeCount + 1,
						// Right
						branchVertexCount - 1, verticeCount + 3, verticeCount + 4,
						branchVertexCount - 1, verticeCount + 4, branchVertexCount
					};
				}
				else
				{
					triangles = new int[]
					{
						// Front
						branchVertexCount - 2, branchVertexCount - 1, verticeCount + 3,
						branchVertexCount - 2, verticeCount + 3, verticeCount + 2, 
						// Left
						branchVertexCount - 2, branchVertexCount - 3, verticeCount + 1,
						branchVertexCount - 2, verticeCount + 1, verticeCount + 2,
						// Back
						branchVertexCount - 3, branchVertexCount, verticeCount + 4,
						branchVertexCount - 3, verticeCount + 4, verticeCount + 1,
						// Right
						branchVertexCount - 1, verticeCount + 3, verticeCount + 4,
						branchVertexCount - 1, verticeCount + 4, branchVertexCount
					};
				}
			}
			else
			{
				triangles = new int[]
				{
					// Front
					verticeCount - 2, verticeCount + 3, verticeCount - 1,
					verticeCount - 2, verticeCount + 2, verticeCount + 3,
					// Left
					verticeCount - 2, verticeCount - 3, verticeCount + 1,
					verticeCount - 2, verticeCount + 1, verticeCount + 2,
					// Back
					verticeCount - 3, verticeCount, verticeCount + 4,
					verticeCount - 3, verticeCount + 4, verticeCount + 1,
					// Right
					verticeCount - 1, verticeCount + 3, verticeCount + 4,
					verticeCount - 1, verticeCount + 4, verticeCount
				};
			}

			return triangles;
		}

		private static int[] GetFrontTriangles(int beforeVerticeCount, int newVerticeCount)
		{
			//TODO: Take direction into account
			return new int[]{
				beforeVerticeCount - 2, newVerticeCount + 3, beforeVerticeCount - 1,
				beforeVerticeCount - 2, newVerticeCount + 2, newVerticeCount + 3
			};
		}

		private static void CloseMesh()
		{
			int verticeCount = mesh.vertexCount - 1;

			int[] triangles = new int[]
			{
				// Top Triangles
				verticeCount - 3, verticeCount, verticeCount - 1,
				verticeCount - 3, verticeCount - 1, verticeCount - 2
			};

			mesh.triangles = mesh.triangles.Concat(triangles).ToArray();
		}

		#endregion
		#endregion
	}

	public class BranchInfo
	{
		public Vector3 currentDirection;
		public Vector3[] lastVertices;
		public int currentVertexCount;
	}


}
