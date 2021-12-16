#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Marvin.LSystemMeshGenerator
{
	[CustomPropertyDrawer(typeof(CharStringDictionary))]
	public class CharStringDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer
	{
	}
}
#endif