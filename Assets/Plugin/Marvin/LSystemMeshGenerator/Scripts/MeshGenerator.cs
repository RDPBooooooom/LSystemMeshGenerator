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

		private static bool isFirstUpAction;

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
			isFirstUpAction = true;

			MeshGenerator.distancePerStep = distancePerStep;
			MeshGenerator.anglePerStep = anglePerStep;
			MeshGenerator.startDiameter = startDiameter;
			MeshGenerator.endDiameter = endDiameter;

			currentDirection = new Vector3(0, 1, 0);

			string source = lSystem.GetLSystemString(iterations);

			Debug.Log(source);

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
						// AngleStep to left
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
			if (isFirstUpAction)
			{
				mesh = GetStartingMesh();
				isFirstUpAction = false;
			}

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

			currentDirection = currentBranchInfo.currentDirection;

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

			if (currentDirection.y > 0)
			{
				startMesh.triangles = new int[]
				{
					0, 1, 2,
					0, 2, 3
				};
			}
			else
			{
				startMesh.triangles = new int[]
				{
					0, 2, 1,
					0, 3, 2
				};
			}


			return startMesh;
		}

		private static Vector3[] GetVertices()
		{
	//		Debug.Log($"================ NEW VERTS ================");
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
		//		Debug.Log($"Vertice {i}: {lastVertices[i]}");
			}

			lastVertices[3] = GetDirectionCorrection(lastVertices[0], lastVertices[3]);
			lastVertices[2] = GetDirectionCorrection(lastVertices[1], lastVertices[2]);

			for (int i = 0; i < lastVertices.Length; i++)
			{
		//		Debug.Log($"Corrected Vertice {i}: {lastVertices[i]}");
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
					// Bottom Left
					return currentDirection.normalized * distancePerStep;
				case 1:
					// Top Left
					return currentDirection.normalized * distancePerStep;
				case 2:
					// Top right
					return currentDirection.normalized * distancePerStep;
				case 3:
					//Bottom right
					return currentDirection.normalized * distancePerStep;
				default:
					return currentDirection.normalized * distancePerStep;
			}
		}

		private static Vector3 GetDirectionCorrection(Vector3 pointOne, Vector3 pointTwo)
		{
			if (currentDirection.normalized.y == 1) return pointTwo;
			if (currentDirection.normalized.y == -1) return pointTwo;

			if (pointOne.z == pointTwo.z && pointTwo.y == pointOne.y)
			{
				float toAdd;
				float diff = pointTwo.y - pointOne.y;

				if (diff < 0)
				{
					toAdd = Mathf.Clamp(1 + diff, 0, 1) * -1;
				}
				else
				{
					toAdd = Mathf.Clamp(1 - diff, 0, 1);
				}

				if (currentDirection.x < 0)
				{
					toAdd *= -1;
				}

				return new Vector3(pointTwo.x, pointTwo.y + toAdd, pointTwo.z);
			}

			return pointTwo;
		}

		private static int[] GetTriangles()
		{
			int verticeCount = mesh.vertexCount - 1;

			List<int> triangleList = new List<int>();

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

				triangleList.AddRange(GetFrontTriangles(branchVertexCount, verticeCount));
				triangleList.AddRange(GetLeftTriangles(branchVertexCount, verticeCount));
				triangleList.AddRange(GetBackTriangles(branchVertexCount, verticeCount));
				triangleList.AddRange(GetRightTriangles(branchVertexCount, verticeCount));
			}
			else
			{
				triangleList.AddRange(GetFrontTriangles(verticeCount, verticeCount));
				triangleList.AddRange(GetLeftTriangles(verticeCount, verticeCount));
				triangleList.AddRange(GetBackTriangles(verticeCount, verticeCount));
				triangleList.AddRange(GetRightTriangles(verticeCount, verticeCount));
			}

			return triangleList.ToArray();
		}

		private static int[] GetFrontTriangles(int beforeVerticeCount, int newVerticeCount)
		{
			if (currentDirection.y > 0)
			{
				return new int[]{
					beforeVerticeCount - 2, newVerticeCount + 3, beforeVerticeCount - 1,
					beforeVerticeCount - 2, newVerticeCount + 2, newVerticeCount + 3
				};
			}
			else
			{
				return new int[]{
					beforeVerticeCount - 2, beforeVerticeCount - 1, newVerticeCount + 3,
					beforeVerticeCount - 2, newVerticeCount + 3, newVerticeCount + 2
				};
			}
		}

		private static int[] GetLeftTriangles(int beforeVerticeCount, int newVerticeCount)
		{
			if (currentDirection.y > 0)
			{
				return new int[]{
					beforeVerticeCount - 2, beforeVerticeCount - 3, newVerticeCount + 1,
					beforeVerticeCount - 2, newVerticeCount + 1, newVerticeCount + 2
				};
			}
			else
			{
				return new int[]{
					beforeVerticeCount - 2, newVerticeCount + 1, beforeVerticeCount - 3,
					beforeVerticeCount - 2, newVerticeCount + 2, newVerticeCount + 1
				};
			}
		}
		private static int[] GetBackTriangles(int beforeVerticeCount, int newVerticeCount)
		{
			if (currentDirection.y > 0)
			{
				return new int[]{
					beforeVerticeCount - 3, beforeVerticeCount, newVerticeCount + 4,
					beforeVerticeCount - 3, newVerticeCount + 4, newVerticeCount + 1
				};
			}
			else
			{
				return new int[]{
					beforeVerticeCount - 3,  newVerticeCount + 4, beforeVerticeCount,
					beforeVerticeCount - 3, newVerticeCount + 1, newVerticeCount + 4
				};
			}
		}

		private static int[] GetRightTriangles(int beforeVerticeCount, int newVerticeCount)
		{
			if (currentDirection.y > 0)
			{
				return new int[]{
					beforeVerticeCount - 1, newVerticeCount + 3, newVerticeCount + 4,
					beforeVerticeCount - 1, newVerticeCount + 4, beforeVerticeCount
				};
			}
			else
			{
				return new int[]{
					beforeVerticeCount - 1,  newVerticeCount + 4, newVerticeCount + 3,
					beforeVerticeCount - 1, beforeVerticeCount, newVerticeCount + 4
				};
			}
		}

		private static void CloseMesh()
		{
			int verticeCount = mesh.vertexCount - 1;
			int[] triangles;

			triangles = new int[]{
				verticeCount - 3, verticeCount, verticeCount - 1,
				verticeCount - 3, verticeCount - 1, verticeCount - 2,
				verticeCount - 3, verticeCount - 1, verticeCount,
				verticeCount - 3, verticeCount - 2, verticeCount - 1
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
