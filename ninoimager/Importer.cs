// -----------------------------------------------------------------------
// <copyright file="Importer.cs" company="none">
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
// <date>17/08/2013</date>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using Ninoimager.Format;
using Drawing = System.Drawing;
using Color = System.Drawing.Color;

namespace Ninoimager
{
	public class Importer
	{
		public Importer()
		{
		}

		public bool IncludePcmp {
			get { return false; }
		}

		public bool IncludeCpos {
			get { return false; }
		}

		public uint DispCnt {
			get { return 0; }
		}

		public uint UnknownChar {
			get { return 0; }
		}

		public Color TransparentColor {
			get { return Color.FromArgb(248, 0, 248); }
		}

		public BgMode BgMode {
			get { return BgMode.Text; }
		}

		public ColorFormat DefaultFormat {
			get { return ColorFormat.Indexed_8bpp; }
		}

		public PixelEncoding PixelEncoding {
			get { return PixelEncoding.HorizontalTiles; }
		}

		/// <summary>
		/// Import a background image creating and writing a NSCR, NCGR and NCLR files to the streams passed.
		/// </summary>
		/// <param name="imgPath">Image path.</param>
		/// <param name="mapStr">Map stream output.</param>
		/// <param name="imgStr">Image stream output.</param>
		/// <param name="palStr">Pal strream output.</param>
		public void ImportBackground(string imgPath, Stream mapStr, Stream imgStr, Stream palStr)
		{
			/* Specifications: 
				+ Image will be HorizontalTiled
				+ Depth will be 8bpp (256/1).
				+ No PCMP block will be added
				+ No CPOS block will be added
				+ Register DISPCNT will be set to 0 (since it's a BG and those bits aren't used)
				+ Unknown value from CHAR block will be set to 0 (since it seems to be used only with OBJ)
				+ BG Mode will be "Text" (most used)
				+ Transparent color will be magenta: (R:248, G:0, B:248) 
			*/

			if (!File.Exists(imgPath) || mapStr == null || imgStr == null || palStr == null)
				throw new ArgumentNullException();

			Drawing.Bitmap bmp = (Drawing.Bitmap)Drawing.Image.FromFile(imgPath);
			int width  = bmp.Width;
			int height = bmp.Height;

			Pixel[] pixels;
			Color[] palette;
			this.GetIndexImage(bmp, out pixels, out palette);

			// Create map from pixels
			Nscr nscr = new Nscr();
			nscr.Width       = width;
			nscr.Height      = height;
			nscr.TileSize    = new Drawing.Size(8, 8);
			nscr.BgMode      = this.BgMode;
			nscr.PaletteMode = (this.DefaultFormat == ColorFormat.Indexed_4bpp) ?
				PaletteMode.Palette16_16 : PaletteMode.Palette256_1;
			pixels = nscr.CreateMap(pixels);

			// Create image format
			Ncgr ncgr = new Ncgr();
			ncgr.Width      = width;
			ncgr.Height     = height;
			ncgr.RegDispcnt = this.DispCnt;
			ncgr.Unknown    = this.UnknownChar;
			ncgr.SetData(pixels, this.PixelEncoding, this.DefaultFormat, new Drawing.Size(8, 8));

			// Create palette format
			Nclr nclr = new Nclr();
			nclr.Extended = false;
			nclr.SetData(palette, this.DefaultFormat);

			// Write data
			nclr.Write(palStr);
			ncgr.Write(imgStr);
			nscr.Write(mapStr);
		}

		private void GetIndexImage(System.Drawing.Bitmap image, out Pixel[] pixels, out Color[] palette)
		{
			List<Color> listColor = new List<Color>();
			int width  = image.Width;
			int height = image.Height;
			Drawing.Size tileSize = new Drawing.Size(8, 8);
			pixels = new Pixel[width * height];

			bool isIndexed = true;	// TODO: Determine if the image is indexed or not.

			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					Color color = image.GetPixel(x, y);
					int index = Image.CalculateTiledIndex(x, y, this.PixelEncoding, width, height, tileSize);

					if (!isIndexed) {
						pixels[index] = new Pixel((uint)color.ToArgb(), color.A, false);
					} else {
						Color noTrans = Color.FromArgb(255, color);

						if (!listColor.Contains(noTrans))
							listColor.Add(noTrans);

						int colorIndex = listColor.IndexOf(noTrans);
						pixels[index] = new Pixel((uint)colorIndex, color.A, true);
					}
				}
			}

			palette = listColor.ToArray();
		}
	}
}
