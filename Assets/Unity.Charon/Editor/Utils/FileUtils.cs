﻿/*
	Copyright (c) 2015 Denis Zykov

	This is part of Charon Game Data Editor Unity Plugin.

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
using System.Threading;
using Assets.Unity.Charon.Editor.Utils;
using UnityEngine;

namespace Assets.Unity.Charon.Editor
{
	static class FileUtils
	{
		public static string MakeProjectRelative(string path)
		{
			if (String.IsNullOrEmpty(path)) return null;
			var oldPath = path;
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

			if (Settings.Current != null && Settings.Current.Verbose && oldPath != path)
				Debug.Log("Making path '" + oldPath + "' relative to project location '" + fullPath + "'. New path is: '" + path + "'.");

			return path;
		}
		public static string ComputeMd5Hash(string path, int tries = 5)
		{
			if (path == null) throw new ArgumentNullException("path");
			if (tries <= 0) throw new ArgumentOutOfRangeException("tries");

			foreach (var attempt in Enumerable.Range(1, tries))
			{
				try
				{
					using (var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
					using (var md5 = new MD5CryptoServiceProvider())
					{
						var hashBytes = md5.ComputeHash(fs);
						return BitConverter.ToString(hashBytes).Replace("-", "");
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
		public static string GetToolsPath()
		{
			var toolsPath = Settings.Current.ToolsPath;
#if !UNITY_EDITOR_WIN
			toolsPath = Settings.Current.MonoPath + " " + Settings.Current.ToolsPath;
#endif
			return toolsPath;
		}
		public static ToolsCheckResult CheckTools()
		{
			if (!File.Exists(Settings.Current.ToolsPath))
				return ToolsCheckResult.MissingTools;
#if !UNITY_EDITOR_WIN
			if (!File.Exists(Settings.Current.MonoPath))
				return ToolsCheckResult.MissingMono;
#endif
			return ToolsCheckResult.Ok;
		}
	}
}
