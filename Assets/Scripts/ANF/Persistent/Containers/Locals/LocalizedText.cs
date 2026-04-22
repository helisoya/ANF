using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ANF.Persistent;

namespace ANF.Locals
{
	/// <summary>
	/// Represents a text that is localized
	/// </summary>
	public class LocalizedText : MonoBehaviour
	{
		[SerializeField] protected string localKey;
		[SerializeField] protected Locals.Channel channel;
		[SerializeField] protected TMP_Text text;
		[SerializeField] protected bool canResize = true;
		[SerializeField] protected bool canRecolor = true;
		[SerializeField] protected bool noText = false;
		protected object[] injectors;
		protected Locals locals;

		void Start()
		{
			if (PersistentDataManager.instance.GetGlobalData().GetComponent<Locals>(out locals))
				locals.RegisterText(this, channel);

			ReloadText();
		}

		protected void OnDestroy()
		{
			if (locals != null)
				locals.UnregisterText(this, channel);
		}

		/// <summary>
		/// Changes the ID of the localized text
		/// </summary>
		/// <param name="key">The new ID</param>
		public void SetNewKey(string key)
		{
			localKey = key;
			ReloadText();
		}

		/// <summary>
		/// Reloads the localized text
		/// </summary>
		public virtual void ReloadText()
		{
			if (noText)
			{
				text.text = "";
				return;
			}

			string txt = localKey;
			if (locals != null)
				txt = locals.GetLocal(localKey);

			if (injectors != null && injectors.Length > 0)
			{
				for (int i = 0; i < injectors.Length; i++)
				{
					txt = txt.Replace(string.Concat("[", i, "]"), injectors[i].ToString());
				}
			}
			text.text = txt;
		}

		/// <summary>
		/// Sets the injectors for this string
		/// </summary>
		/// <param name="newInjectors">The new injectors</param>
		/// <param name="reloadText">True if the text should be immediatly reloaded</param>
		public void SetInjectors(object[] newInjectors, bool reloadText = true)
		{
			injectors = newInjectors;
			if (reloadText) ReloadText();
		}

		/// <summary>
		/// Sets the current font for the text
		/// </summary>
		/// <param name="font">The new font</param>
		public void SetFont(TMP_FontAsset font)
		{
			text.font = font;
		}

		/// <summary>
		/// Sets if the text should be shown or not
		/// </summary>
		/// <param name="textActive">True if the text is shown</param>
		/// <param name="reloadText">True if the visual should be reloaded</param>
		public void SetNoText(bool textActive, bool reloadText = true)
		{
			noText = textActive;
			if (reloadText) ReloadText();
		}

		/// <summary>
		/// Returns the text field
		/// </summary>
		/// <returns>The text field</returns>
		public TMP_Text GetText()
		{
			return text;
		}

		/// <summary>
		/// Changes the text's color
		/// </summary>
		/// <param name="color">The new color</param>
		public void SetColor(Color color)
		{
			if (!canRecolor) return;

			text.color = color;
		}

		/// <summary>
		/// Sets the font size
		/// </summary>
		/// <param name="size">The new size</param>
		public void SetSize(int size)
		{
			if (!canResize) return;

			text.fontSize = size;
			text.fontSizeMax = size;
		}

		public string Key { get { return localKey; } }
		public Locals.Channel Channel { get { return channel; } }
	}
}
