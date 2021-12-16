using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Marvin.LSystemMeshGenerator
{
	/// <summary>
	/// Class used to generate a Mesh with a LSystem
	/// </summary>
	public class MeshGenerator
	{
		#region Variables

		private Mesh mesh;

		private float distancePerStep;

		private float anglePerStep;

		private float startDiameter;

		private float endDiameter;

		private Vector3 currentDirection;

		private Stack<BranchInfo> branchStack;

		private BranchInfo currentBranchInfo;

		private bool isFirstUpAction;

		#endregion

		#region Mesh Generation

		/// <summary>
		/// Generates a mesh based on the given LSystem with the number of iterations 
		/// <br>Uses standart values for:</br>
		/// <br>distancePerStep: 1</br>
		/// <br>anglePerStep: 45</br>
		/// <br>startDiameter: 1</br>
		/// <br>endDiameter: 1</br>
		/// </summary>
		/// <param name="lSystem">The LSystem to use</param>
		/// <param name="iterations">Number of iterations the LSystem should be used with</param>
		/// <returns>The generated mesh</returns>
		public virtual Mesh GenerateMesh(LSystem lSystem, int iterations)
		{
			return GenerateMesh(lSystem, iterations, 1, 45, 1, 1);
		}

		/// <summary>
		/// Generates a mesh based on the given LSystem with the number of iterations 
		/// </summary>
		/// <param name="lSystem">The LSystem to use</param>
		/// <param name="iterations">Number of iterations the LSystem should be used with</param>
		/// <param name="distancePerStep">The distance the mesh extends per step</param>
		/// <param name="anglePerStep">The degree the direction of the mesh generation changes per angle change</param>
		/// <param name="startDiameter">The diameter at the start of the mesh</param>
		/// <param name="endDiameter">The diameter at the end of the mesh</param>
		/// <returns>The generated mesh</returns>
		public virtual Mesh GenerateMesh(LSystem lSystem, int iterations, float distancePerStep, float anglePerStep, float startDiameter, float endDiameter)
		{
			// Setup
			branchStack = new Stack<BranchInfo>();
			currentBranchInfo = null;
			isFirstUpAction = true;

			this.distancePerStep = distancePerStep;
			this.anglePerStep = anglePerStep;
			this.startDiameter = startDiameter;
			this.endDiameter = endDiameter;

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
						// Do Nothing Debug.Log("illegal L System Operation: " + key);
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
		/// <summary>
		/// Extends the mesh in the current direction by one distancePerStep
		/// </summary>
		protected virtual void UpAction()
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

		/// <summary>
		/// Changes the current direction
		/// </summary>
		/// <param name="isPositiv">If the rotation should be positiv</param>
		/// <param name="axis">The axisfor the rotation</param>
		protected virtual void AngleUpdateAction(bool isPositiv, Vector3 axis)
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

		/// <summary>
		/// Starts a new branch and stores the current direction, position and the last vertices.
		/// </summary>
		protected virtual void StartBranchAction()
		{
			BranchInfo currentInfo = new BranchInfo
			{
				currentDirection = currentDirection,
				currentVertexCount = mesh.vertexCount,
				lastVertices = GetLastVerticesFromMesh(4)
			};

			branchStack.Push(currentInfo);
		}

		/// <summary>
		/// Closes the mesh for the current branch and loads the data of the last opened branch
		/// </summary>
		protected virtual void EndBranchAction()
		{
			CloseMesh();
			currentBranchInfo = branchStack.Pop();

			currentDirection = currentBranchInfo.currentDirection;
		}

		#endregion

		#region Mesh gen. helper methods

		/// <summary>
		/// Creates the starting point for the mesh
		/// </summary>
		/// <returns>The new starting mesh</returns>
		protected virtual Mesh GetStartingMesh()
		{
			Mesh startMesh = new Mesh();

			float lenght = GetCubeLength(startDiameter);

			startMesh.vertices = new Vector3[]
				{
					new Vector3(0, 0, 0),
					new Vector3(lenght, 0, 0),
					new Vector3(lenght, 0, lenght),
					new Vector3(0, 0, lenght),
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

		/// <summary>
		/// Calculates the lenght of one side of a cube based on the <paramref name="diameter"/>
		/// </summary>
		/// <param name="diameter">The diameter to calculate the length for</param>
		/// <returns>Calculated length of one side</returns>
		protected virtual float GetCubeLength(float diameter)
		{
			return Mathf.Sqrt((diameter * diameter) / 2);
		}

		/// <summary>
		/// Get the next 4 vertices based the current direction and the distancePerStep
		/// </summary>
		/// <returns>The next 4 vertices</returns>
		protected virtual Vector3[] GetVertices()
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
				lastVertices[i] = GetForwardWithDirection(lastVertices[i]);
			}

			lastVertices[3] = GetDirectionCorrection(lastVertices[0], lastVertices[3]);
			lastVertices[2] = GetDirectionCorrection(lastVertices[1], lastVertices[2]);

			return lastVertices;
		}

		/// <summary>
		/// Gets the last <paramref name="count"/> vertices from the current mesh
		/// </summary>
		/// <param name="count">Number of vertices to get</param>
		/// <returns>Last x vertices of the current mesh</returns>
		protected virtual Vector3[] GetLastVerticesFromMesh(int count)
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

		/// <summary>
		/// Change the <paramref name="toChange"/> vector based on the current direction and distancePerStep
		/// </summary>
		/// <param name="toChange">Point to change</param>
		/// <returns>The changed vector</returns>
		protected virtual Vector3 GetForwardWithDirection(Vector3 toChange)
		{
			return currentDirection.normalized * distancePerStep + toChange;
		}

		/// <summary>
		/// Pushes the points away from each other if they are to close.
		/// </summary>
		/// <param name="pointOne">PointOne to check</param>
		/// <param name="pointTwo">PointTwo to correct</param>
		/// <returns>The corrected <paramref name="pointTwo"/></returns>
		protected virtual Vector3 GetDirectionCorrection(Vector3 pointOne, Vector3 pointTwo)
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

		/// <summary>
		/// Get the next Triangles for the last 4 vertices
		/// </summary>
		/// <returns>The next triangles for the mesh</returns>
		protected virtual int[] GetTriangles()
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

		/// <summary>
		/// Get Triangles for the front
		/// </summary>
		/// <param name="beforeVerticeCount">Vertice count for the last 4 vertices (before upAction)</param>
		/// <param name="newVerticeCount">vertice count for the next 4 vertices</param>
		/// <returns>The front triangles</returns>
		protected virtual int[] GetFrontTriangles(int beforeVerticeCount, int newVerticeCount)
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

		/// <summary>
		/// Get Triangles for the left
		/// </summary>
		/// <param name="beforeVerticeCount">Vertice count for the last 4 vertices (before upAction)</param>
		/// <param name="newVerticeCount">vertice count for the next 4 vertices</param>
		/// <returns>The left triangles</returns>
		protected virtual int[] GetLeftTriangles(int beforeVerticeCount, int newVerticeCount)
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

		/// <summary>
		/// Get Triangles for the back
		/// </summary>
		/// <param name="beforeVerticeCount">Vertice count for the last 4 vertices (before upAction)</param>
		/// <param name="newVerticeCount">vertice count for the next 4 vertices</param>
		/// <returns>The back triangles</returns>
		protected virtual int[] GetBackTriangles(int beforeVerticeCount, int newVerticeCount)
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

		/// <summary>
		/// Get Triangles for the right
		/// </summary>
		/// <param name="beforeVerticeCount">Vertice count for the last 4 vertices (before upAction)</param>
		/// <param name="newVerticeCount">vertice count for the next 4 vertices</param>
		/// <returns>The right triangles</returns>
		protected virtual int[] GetRightTriangles(int beforeVerticeCount, int newVerticeCount)
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

		/// <summary>
		/// Creates the end of a mesh. Creates 4 new vertices like Upaction but with half DistancePerStep. Connects the top 4 vertices to close the mesh
		/// </summary>
		protected virtual void CloseMesh()
		{
			Vector3[] vertices = GetVertices();
			int[] wallTriangels = GetTriangles();

			float lenghtDiff = GetCubeLength(startDiameter) - GetCubeLength(endDiameter);
			// TODO: Ugly -> find better solution
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i] = vertices[i] - currentDirection.normalized * (distancePerStep / 2);

				if (i == 1)
				{
					vertices[i] = new Vector3(vertices[i].x - lenghtDiff, vertices[i].y, vertices[i].z);
				}
				else if (i == 2)
				{
					vertices[i] = new Vector3(vertices[i].x - lenghtDiff, vertices[i].y, vertices[i].z - lenghtDiff);
				}
				else if (i == 3)
				{
					vertices[i] = new Vector3(vertices[i].x, vertices[i].y, vertices[i].z - lenghtDiff);
				}
			}

			mesh.vertices = mesh.vertices.Concat(vertices).ToArray();
			mesh.triangles = mesh.triangles.Concat(wallTriangels).ToArray();

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

	/// <summary>
	/// Holds the information of a state in the LSystem. Used to store the current state of a LSystem when a new Branch is created
	/// </summary>
	public class BranchInfo
	{
		public Vector3 currentDirection;
		public Vector3[] lastVertices;
		public int currentVertexCount;
	}

}
