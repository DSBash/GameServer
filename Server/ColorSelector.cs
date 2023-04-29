// Copyright (c) 2012, Yves Goergen, http://unclassified.de
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, are permitted
// provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this list of conditions
//   and the following disclaimer.
// * Redistributions in binary form must reproduce the above copyright notice, this list of
//   conditions and the following disclaimer in the documentation and/or other materials provided
//   with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
// IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
// OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Unclassified;
using Unclassified.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Unclassified.UI
{
	public partial class ColorSelector : UserControl
	{
		#region Private fields
		private bool updatingFaders;
		private bool updatingRGBText;
		private bool updatingHexText;
		private Color selectedColor;
		#endregion Private fields

		#region Events
		public event EventHandler SelectedColorChanged;
		public event EventHandler CloseRequested;
		public event EventHandler CancelRequested;
		#endregion Events

		#region Localisation
		public string T_ExtendedView
		{
			get { return ExtendedViewCheck.Text; }
			set { ExtendedViewCheck.Text = value; }
		}
		
		public string T_OK
		{
			get { return OKActionButton.Text; }
			set { OKActionButton.Text = value; }
		}
		
		public string T_Cancel
		{
			get { return CancelActionButton.Text; }
			set { CancelActionButton.Text = value; }
		}
		
		public string T_Red
		{
			get { return RedLabel.Text; }
			set { RedLabel.Text = value; }
		}
		
		public string T_Green
		{
			get { return GreenLabel.Text; }
			set { GreenLabel.Text = value; }
		}
		
		public string T_Blue
		{
			get { return BlueLabel.Text; }
			set { BlueLabel.Text = value; }
		}
		
		public string T_Hue
		{
			get { return HueLabel.Text; }
			set { HueLabel.Text = value; }
		}
		
		public string T_Saturation
		{
			get { return SaturationLabel.Text; }
			set { SaturationLabel.Text = value; }
		}
		
		public string T_Lightness
		{
			get { return LightnessLabel.Text; }
			set { LightnessLabel.Text = value; }
		}
		
		public string T_CSSrgb
		{
			get { return CSSrgbLabel.Text; }
			set { CSSrgbLabel.Text = value; }
		}
		
		public string T_HTML
		{
			get { return HTMLLabel.Text; }
			set { HTMLLabel.Text = value; }
		}

		public void AutoLocalise()
		{
			switch (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
			{
				case "de":
					T_ExtendedView = "Erweiterte Ansicht";
					T_OK = "OK";
					T_Cancel = "Abbrechen";
					T_Red = "Rot:";
					T_Green = "Grün:";
					T_Blue = "Blau:";
					T_Hue = "Farbton:";
					T_Saturation = "Sättigung:";
					T_Lightness = "Helligkeit:";
					T_CSSrgb = "CSS rgb:";
					T_HTML = "HTML:";
					break;
				case "en":
				default:
					T_ExtendedView = "Extended view";
					T_OK = "OK";
					T_Cancel = "Cancel";
					T_Red = "Red:";
					T_Green = "Green:";
					T_Blue = "Blue:";
					T_Hue = "Hue:";
					T_Saturation = "Saturation:";
					T_Lightness = "Lightness:";
					T_CSSrgb = "CSS rgb:";
					T_HTML = "HTML:";
					break;
				case "es":
					T_ExtendedView = "Vista extendida";
					T_OK = "Aceptar";
					T_Cancel = "Cancelar";
					T_Red = "Rojo:";
					T_Green = "Verde:";
					T_Blue = "Azul:";
					T_Hue = "Matiz:";
					T_Saturation = "Saturación:";
					T_Lightness = "Luminosidad:";
					T_CSSrgb = "CSS rgb:";
					T_HTML = "HTML:";
					break;
				case "fr":
					T_ExtendedView = "Vue étendue";
					T_OK = "OK";
					T_Cancel = "Annuler";
					T_Red = "Rouge:";
					T_Green = "Vert:";
					T_Blue = "Bleu:";
					T_Hue = "Teinte:";
					T_Saturation = "Saturation:";
					T_Lightness = "Luminosité:";
					T_CSSrgb = "CSS rgb:";
					T_HTML = "HTML:";
					break;
				case "nl":
					T_ExtendedView = "Extended view";
					T_OK = "OK";
					T_Cancel = "Annuleren";
					T_Red = "Rood:";
					T_Green = "Groen:";
					T_Blue = "Blauw:";
					T_Hue = "Tint:";
					T_Saturation = "Verzadiging:";
					T_Lightness = "Helderheid:";
					T_CSSrgb = "CSS rgb:";
					T_HTML = "HTML:";
					break;
			}
		}
		#endregion Localisation

		#region Public properties
		public Color SelectedColor
		{
			get
			{
				return selectedColor;
			}
			set
			{
				if (value != selectedColor)
				{
					selectedColor = value;

					if (!updatingFaders)
					{
						faderRed.Ratio = value.R;
						faderGreen.Ratio = value.G;
						faderBlue.Ratio = value.B;
					}

					UpdateColors();

					if (SelectedColorChanged != null)
					{
						SelectedColorChanged(this, EventArgs.Empty);
					}
				}
			}
		}

		public string RgbText
		{
			get { return textRGB.Text; }
			set { textRGB.Text = value; }
		}

		public string HtmlText
		{
			get { return textHex.Text; }
			set { textHex.Text = value; }
		}
		#endregion Public properties

		#region Constructors
		public ColorSelector()
		{
			InitializeComponent();

			AutoLocalise();

			RGBfader_RatioChanged(null, null);
		}
		#endregion Constructors

		#region Control event handlers
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ExtendedViewCheck_CheckedChanged(null, null);
		}
		#endregion Control event handlers

		#region Sub-control event handlers
		private void RGBfader_RatioChanged(object sender, EventArgs e)
		{
			if (!updatingFaders)
			{
				updatingFaders = true;

				Color rgb = Color.FromArgb(faderRed.Ratio, faderGreen.Ratio, faderBlue.Ratio);
				HslColor hsl = ColorMath.RgbToHsl(rgb);

				faderHue.Ratio = hsl.H;
				faderSaturation.Ratio = hsl.S;
				faderLightness.Ratio = hsl.L;

				SelectedColor = rgb;

				updatingFaders = false;
			}
		}

		private void HSLfader_RatioChanged(object sender, EventArgs e)
		{
			if (!updatingFaders)
			{
				updatingFaders = true;

				HslColor hsl = new HslColor(faderHue.Ratio, faderSaturation.Ratio, faderLightness.Ratio);
				Color rgb = ColorMath.HslToRgb(hsl);

				faderRed.Ratio = rgb.R;
				faderGreen.Ratio = rgb.G;
				faderBlue.Ratio = rgb.B;

				SelectedColor = rgb;

				updatingFaders = false;
			}
		}

		private void colorWheel1_HueChanged(object sender, EventArgs e)
		{
			updatingFaders = true;

			faderHue.Ratio = colorWheel1.Hue;

			HslColor hsl = new HslColor(faderHue.Ratio, faderSaturation.Ratio, faderLightness.Ratio);
			Color rgb = ColorMath.HslToRgb(hsl);

			faderRed.Ratio = rgb.R;
			faderGreen.Ratio = rgb.G;
			faderBlue.Ratio = rgb.B;

			SelectedColor = rgb;

			updatingFaders = false;
		}

		private void colorWheel1_SLChanged(object sender, EventArgs e)
		{
			updatingFaders = true;

			faderSaturation.Ratio = colorWheel1.Saturation;
			faderLightness.Ratio = colorWheel1.Lightness;

			HslColor hsl = new HslColor(faderHue.Ratio, faderSaturation.Ratio, faderLightness.Ratio);
			Color rgb = ColorMath.HslToRgb(hsl);

			faderRed.Ratio = rgb.R;
			faderGreen.Ratio = rgb.G;
			faderBlue.Ratio = rgb.B;

			SelectedColor = rgb;

			updatingFaders = false;
		}

		private void textRGB_TextChanged(object sender, EventArgs e)
		{
			if (!updatingRGBText)
			{
				updatingRGBText = true;

				try
				{
					Match m = Regex.Match(textRGB.Text.Trim().ToLower(), @"^rgb\(\s*([0-9]{1,3})\s*,\s*([0-9]{1,3})\s*,\s*([0-9]{1,3})\s*\)$");
					if (m.Success)
					{
						int r = int.Parse(m.Groups[1].Value);
						int g = int.Parse(m.Groups[2].Value);
						int b = int.Parse(m.Groups[3].Value);
						if (r > 255) throw new FormatException();
						if (g > 255) throw new FormatException();
						if (b > 255) throw new FormatException();
						faderRed.Ratio = (byte) r;
						faderGreen.Ratio = (byte) g;
						faderBlue.Ratio = (byte) b;
					}
					else
					{
						throw new FormatException();
					}
					textRGB.BackColor = Color.Empty;
				}
				catch (FormatException)
				{
					textRGB.BackColor = ColorMath.Blend(SystemColors.Window, Color.Crimson, 0.3);
				}

				updatingRGBText = false;
			}
		}

		private void textHex_TextChanged(object sender, EventArgs e)
		{
			if (!updatingHexText)
			{
				updatingHexText = true;

				try
				{
					if (textHex.Text.Length == 0)
						throw new FormatException();
					if (textHex.Text[0] == '#' && textHex.Text.Length != 4 && textHex.Text.Length != 7)
						throw new FormatException();

					Color c = ColorTranslator.FromHtml(textHex.Text);
					faderRed.Ratio = c.R;
					faderGreen.Ratio = c.G;
					faderBlue.Ratio = c.B;

					textHex.BackColor = Color.Empty;
				}
				catch (Exception)
				{
					textHex.BackColor = ColorMath.Blend(SystemColors.Window, Color.Crimson, 0.3);
				}

				updatingHexText = false;
			}
		}

		private void picturePalette_Click(object sender, EventArgs e)
		{
			Control c = sender as Control;
			Color color = c.BackColor;
			faderRed.Ratio = color.R;
			faderGreen.Ratio = color.G;
			faderBlue.Ratio = color.B;

			if (!ExtendedViewCheck.Checked)
			{
				if (CloseRequested != null)
				{
					CloseRequested(this, EventArgs.Empty);
				}
			}
		}

		private void ExtendedViewCheck_CheckedChanged(object sender, EventArgs e)
		{
			tableLayoutPanel1.SuspendLayout();
			ActionsPanel.Visible = ExtendedViewCheck.Checked;
			foreach (Control control in tableLayoutPanel1.Controls)
			{
				if (tableLayoutPanel1.GetColumn(control) > 0)
				{
					control.Visible = ExtendedViewCheck.Checked;
				}
			}
			tableLayoutPanel1.ResumeLayout();
		}

		private void OKActionButton_Click(object sender, EventArgs e)
		{
			if (CloseRequested != null)
			{
				CloseRequested(this, EventArgs.Empty);
			}
		}

		private void CancelActionButton_Click(object sender, EventArgs e)
		{
			if (CancelRequested != null)
			{
				CancelRequested(this, EventArgs.Empty);
			}
		}
		#endregion Sub-control event handlers

		#region Management methods
		private void UpdateColors()
		{
			Color c = Color.FromArgb(faderRed.Ratio, faderGreen.Ratio, faderBlue.Ratio);

			faderRed.Color1 = Color.FromArgb(0, c.G, c.B);
			faderRed.Color2 = Color.FromArgb(255, c.G, c.B);

			faderGreen.Color1 = Color.FromArgb(c.R, 0, c.B);
			faderGreen.Color2 = Color.FromArgb(c.R, 255, c.B);

			faderBlue.Color1 = Color.FromArgb(c.R, c.G, 0);
			faderBlue.Color2 = Color.FromArgb(c.R, c.G, 255);

			HslColor hsl = new HslColor(faderHue.Ratio, faderSaturation.Ratio, faderLightness.Ratio);

			faderSaturation.Color1 = ColorMath.HslToRgb(new HslColor(hsl.H, 0, hsl.L));
			faderSaturation.Color2 = ColorMath.HslToRgb(new HslColor(hsl.H, 255, hsl.L));

			faderLightness.Color1 = ColorMath.HslToRgb(new HslColor(hsl.H, hsl.S, 0));
			faderLightness.ColorMid = ColorMath.HslToRgb(new HslColor(hsl.H, hsl.S, 128));
			faderLightness.Color2 = ColorMath.HslToRgb(new HslColor(hsl.H, hsl.S, 255));

			if (!updatingRGBText)
			{
				updatingRGBText = true;
				textRGB.Text = "rgb(" + c.R + ", " + c.G + ", " + c.B + ")";
				textRGB.BackColor = Color.Empty;
				updatingRGBText = false;
			}

			if (!updatingHexText)
			{
				updatingHexText = true;
				textHex.Text = "#" + c.R.ToString("x2") + c.G.ToString("x2") + c.B.ToString("x2");
				textHex.BackColor = Color.Empty;
				updatingHexText = false;
			}

			colorWheel1.Hue = hsl.H;
			colorWheel1.Saturation = hsl.S;
			colorWheel1.Lightness = hsl.L;
		}
		#endregion Management methods
	}
}
