using System;
using UnityEngine;

namespace Marvin.LSystemMeshGenerator
{
	/// <summary>
	/// A RuleSet for a LSystem. This class only saves the rules and is not applying them.
	[Serializable]
	public class LSystemRuleSet : ScriptableObject
	{

		[Header("Rules <char, string>")]
		[SerializeField]
		protected CharStringDictionary rules;

		/// <summary>
		/// Set/Get the rules as a <Char, String> Dictionary. Uses CharStringDictionary to make it displayable in the inspector.
		/// </summary>
		public CharStringDictionary Rules { get => rules; private set => rules = value; }

	}

}
