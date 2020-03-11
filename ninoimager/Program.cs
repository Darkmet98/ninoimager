// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="none">
// Copyright (C) 2019
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
// <date>28/05/2019</date>
// -----------------------------------------------------------------------
namespace Ninoimager
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Ninoimager.Cli;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;

    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("ninoimager ~~ Image importer and exporter for Ni no kuni DS");
            Console.WriteLine("V {0} ~~ by pleoNeX ~~", Assembly.GetExecutingAssembly().GetName().Version);
            
            Console.WriteLine();

            if (args.Length == 0) {
                Console.WriteLine("Export: -export \"File.nclr\" \"file.ncgr\" \"file.ncsr\" \"file.png\"");
                Environment.Exit(1);
            }

            if (args[0] == "-export")
            {
                ExportImage(args[1], args[2], args[3], args[4]);
                Console.WriteLine("La operación ha finalizado.");
            }

        }


        public static void ExportImage(string palette, string tile, string maps, string output)
        {
            var export = new ExportMultiNscr
            {
                InputPalette = palette, InputMaps = new[] {maps}, InputTiles = tile, Output = output
            };
            var run = new Runner(export, "exp");
            run.Run();

        }
    }
}