﻿// -----------------------------------------------------------------------
// <copyright file="ManyFixedPaletteQuantization.cs" company="none">
// Copyright (C) 2014 
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
// <date>28/06/2014</date>
// -----------------------------------------------------------------------
using System;
using System.Linq;
using Ninoimager.Format;
using LabColor  = Emgu.CV.Structure.Lab;
using Color     = Emgu.CV.Structure.Bgra;
using EmguImage = Emgu.CV.Image<Emgu.CV.Structure.Bgra, System.Byte>;

namespace Ninoimager.ImageProcessing
{
	public class ManyFixedPaletteQuantization : FixedPaletteQuantization
	{
		private ColorQuantization compareQuantization;
		private Color[][] palettes;
		private LabColor[][] labPalettes;
		private EmguImage image;

		public ManyFixedPaletteQuantization(Color[][] palettes)
			: base(palettes[0])
		{
			this.compareQuantization = new BasicQuantization();
			this.palettes = palettes;

			if (palettes.Length > 1) {
				this.labPalettes = new LabColor[palettes.Length][];
				for (int i = 0; i < palettes.Length; i++)
					labPalettes[i] = ColorConversion.ToLabPalette<Color>(palettes[i]);
			}
		}

		public int SelectedPalette {
			get;
			private set;
		}

		protected override void PreQuantization(Emgu.CV.Image<Color, byte> image)
		{
			this.image = image;

			// If only there is one, do nothing
			if (this.palettes.Length == 1) {
				base.PreQuantization(image);
				return;
			}

			// Extract a palette from the image removing transparent colors (not approximated)
			this.compareQuantization.TileSize = this.TileSize;
			this.compareQuantization.Quantizate(image);
			Color[] comparePalette = this.compareQuantization.Palette.
				Where(c => c.Alpha == 255).ToArray();
			LabColor[] compareLabPalette = ColorConversion.ToLabPalette<Color>(comparePalette);

			// Compare all possible palettes to get the similar one
			double minDistance = Double.MaxValue;
			for (int i = 0; i < palettes.Length && minDistance > 0; i++) {
				double distance = PaletteDistance.CalculateDistance(
					compareLabPalette, this.labPalettes[i]);
				if (distance < minDistance) {
					this.SelectedPalette = i;
					minDistance = distance;
				}
			}

			// Set the palette...
			this.Palette = this.palettes[this.SelectedPalette];

			// ... and run the FixedPaletteQuantization
			base.PreQuantization(image);
		}

		protected override Pixel QuantizatePixel(int x, int y) {
			// If it's a transparent color, set the first palette color
			if (this.image[y, x].Alpha == 0)
				return new Pixel(0, 0, true);
			else
				return base.QuantizatePixel(x, y);
		}
	}
}

