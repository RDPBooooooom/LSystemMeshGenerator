using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Marvin.LSystemMeshGenerator
{

	/// <summary>
	/// Class to create a String based on an LSystemRule and the seed
	/// </summary>
	[Serializable]
	public class LSystem : ScriptableObject
	{
		#region Fields

		[SerializeField]
		protected string seed;

		[SerializeField]
		protected LSystemRuleSet ruleSet;

		#endregion

		#region Properties

		/// <summary>
		/// Seed of this LSystem
		/// </summary>
		public string Seed { get => seed; private set => seed = value; }

		/// <summary>
		/// Rules for this LSystem
		/// </summary>
		public LSystemRuleSet RuleSet { get => ruleSet; set => ruleSet = value; }
		#endregion

		/// <summary>
		/// Creates the result of the L-System with the given seed and ruleSet.
		/// </summary>
		/// <param name="iterations">How many times the rule set should be applied to the seed</param>
		/// <returns>Result of the L-System with the definied number of iterations</returns>
		public virtual string GetLSystemString(int iterations)
		{
			string currentString = seed;

			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < iterations; i++)
			{
				foreach (char c in currentString)
				{
					sb.Append(RuleSet.Rules.ContainsKey(c) ? RuleSet.Rules[c] : c.ToString());
				}
				currentString = sb.ToString();
				sb = new StringBuilder();
			}
			return currentString;
		}

	}

}

