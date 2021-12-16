using System;

namespace Marvin.LSystemMeshGenerator
{

	/// <summary>
	/// A Unity inspector serializeable version of a dictionary of type key:char, value:string
	/// Depends on (Unity Asset Store URL): https://assetstore.unity.com/packages/tools/integration/serializabledictionary-90477
	/// </summary>
	[Serializable]
	public class CharStringDictionary : SerializableDictionary<char, string>
	{ }
}
