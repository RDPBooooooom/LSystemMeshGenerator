using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Marvin.LSystemMeshGenerator
{
	[Serializable]
	public class LSystemRuleSet : ScriptableObject
	{

		[Header("Rules <char, string>")]

		[SerializeField]
		private CharStringDictionary rules;

		public CharStringDictionary Rules { get => rules; private set => rules = value; }

	}

}
