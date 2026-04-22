using System;
using TMPro;
using UnityEngine;

namespace ANF.Locals
{
	/// <summary>
	/// Represents the base settings of the local system
	/// </summary>
	[System.Serializable]
	public class LocalsData
	{
		[Header("General")]
		public string[] languages = new string[] { "eng" };
		public TMP_FontAsset[] fonts;
		public int[] sizes = new int[] { 16, 18, 20 };

		[Header("Default Settings")]
		public string defaultLanguage = "eng";
		public LocalChannelData[] defaultData = new LocalChannelData[Enum.GetValues(typeof(Locals.Channel)).Length];
	}
}
