﻿/*
	Copyright (c) 2017 Denis Zykov

	This is part of "Charon: Game Data Editor" Unity Plugin.

	Charon Game Data Editor Unity Plugin is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses.
*/

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEngine;

namespace GameDevWare.Charon.Utils
{
	internal static class FileAndPathUtils
	{
		private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

		public static string MakeProjectRelative(string path)
		{
			if (string.IsNullOrEmpty(path)) return null;
			var fullPath = Path.GetFullPath(Environment.CurrentDirectory).Replace("\\", "/");
			path = Path.GetFullPath(path).Replace("\\", "/");

			if (path[path.Length - 1] == Path.DirectorySeparatorChar || path[path.Length - 1] == Path.DirectorySeparatorChar)
				path = path.Substring(0, path.Length - 1);
			if (fullPath[fullPath.Length - 1] == Path.DirectorySeparatorChar || fullPath[fullPath.Length - 1] == Path.DirectorySeparatorChar)
				fullPath = fullPath.Substring(0, fullPath.Length - 1);

			if (path == fullPath)
				path = ".";
			else if (path.StartsWith(fullPath, StringComparison.Ordinal))
				path = path.Substring(fullPath.Length + 1);
			else
				path = null;

			return path;
		}
		public static string ComputeHash(string path, string hashAlgorithmName = "MD5", int tries = 5)
		{
			if (path == null) throw new ArgumentNullException("path");
			if (tries <= 0) throw new ArgumentOutOfRangeException("tries");

			foreach (var attempt in Enumerable.Range(1, tries))
			{
				try
				{
					using (var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
					using (var hashAlgorithm = HashAlgorithm.Create(hashAlgorithmName) ?? new MD5CryptoServiceProvider())
					{
						var hashBytes = hashAlgorithm.ComputeHash(fs);
						return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
					}
				}
				catch (IOException exception)
				{
					if (Settings.Current.Verbose)
						Debug.LogWarning("Attempt #" + attempt + " to compute hash of " + path + " has failed with IO error: " + exception);

					if (attempt == tries)
						throw;
				}
				Thread.Sleep(100);
			}

			return new string('0', 32); // never happens
		}


		public static string SanitizeFileName(string path)
		{
			var fileName = new StringBuilder(path);
			for (var c = 0; c < fileName.Length; c++)
			{
				if (Array.IndexOf(InvalidFileNameChars, fileName[c]) != -1)
					fileName[c] = '_';
			}
			return fileName.ToString();
		}
		internal static string GetProgramFilesx86()
		{
			if (8 == IntPtr.Size || (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
			{
				return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
			}

			return Environment.GetEnvironmentVariable("ProgramFiles");
		}
	}
}
