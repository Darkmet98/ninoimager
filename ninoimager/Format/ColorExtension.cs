// -----------------------------------------------------------------------
// <copyright file="ColorExtension.cs" company="none">
// Copyright (C) 2013
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by 
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful, 
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details. 
//
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see "http://www.gnu.org/licenses/". 
// </copyright>
// <author>pleoNeX</author>
// <email>benito356@gmail.com</email>
// <date>20/08/2013</date>
// -----------------------------------------------------------------------
using System;
using Color = Emgu.CV.Structure.Rgba;

namespace Ninoimager.Format
{
	public static class ColorExtension
	{
		public static int CompareTo(this Color c1, Color c2)
		{
			if (c1.Red == c2.Red) {
				if (c1.Green == c2.Green) {
					return c1.Blue.CompareTo(c2.Blue);
				} else {
					return c1.Green.CompareTo(c2.Green);
				}
			} else {
				return c1.Red.CompareTo(c2.Red);
			}
		}

		public static Color[] ToArgbColors(this uint[] argb)
		{
			Color[] colors = new Color[argb.Length];
			for (int i = 0; i < argb.Length; i++)
				colors[i] = argb[i].ToArgbColor();
			return colors;
		}

		public static Color ToArgbColor(this uint argb)
		{
			return new Color(
				(argb >> 16) & 0xFF,
				(argb >> 08) & 0xFF,
				(argb >> 00) & 0xFF,
				(argb >> 24) & 0xFF
			);
		}

		public static ushort ToBgr555Color(this Color color)
		{
			int red   = (int)(color.Red   / 8);
			int green = (int)(color.Green / 8);
			int blue  = (int)(color.Blue  / 8);

			return (ushort)((red << 0) | (green << 5) | (blue << 10));
		}

		public static byte[] ToBgr555Colors(this Color[] colors)
		{
			byte[] values = new byte[colors.Length * 2];

			for (int i = 0; i < colors.Length; i++) {
				ushort bgr = colors[i].ToBgr555Color();
				Array.Copy(BitConverter.GetBytes(bgr), 0, values, i * 2, 2);
			}

			return values;
		}

		public static Color ToBgr555Color(this ushort value)
		{
			double red   = ((value & 0x001F) >> 00) * 8;
			double green = ((value & 0x03E0) >> 05) * 8;
			double blue  = ((value & 0x7C00) >> 10) * 8;

			return new Color(red, green, blue, 255);
		}

		public static Color[] ToBgr555Colors(this byte[] values)
		{
			if (values.Length % 2 != 0)
				throw new ArgumentException("Length must be even.");

			Color[] colors = new Color[values.Length / 2];
			for (int i = 0; i < colors.Length; i++) {
				colors[i] = BitConverter.ToUInt16(values, i*2).ToBgr555Color();
			}

			return colors;
		}
	}
}

