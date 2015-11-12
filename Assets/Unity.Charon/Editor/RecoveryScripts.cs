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
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.Unity.Charon.Editor
{
	class RecoveryScripts
	{
#if UNITY_EDITOR_WIN
		private const string RECOVERYSCRIPTS_PATH = "./Library/GenerateGameDataCode.bat";
#else
		private const string RECOVERYSCRIPTS_PATH = "./Library/GenerateGameDataCode.sh";
#endif

		public static void Clear()
		{
			if (File.Exists(RECOVERYSCRIPTS_PATH))
				File.Delete(RECOVERYSCRIPTS_PATH);
		}
		public static void Generate()
		{
			try
			{
				var output = new StringBuilder();
				var paths = Settings.Current.GameDataPaths.ToArray();
				for (var i = 0; i < paths.Length; i++)
				{
					var gameDataPath = paths[i];
					if (File.Exists(gameDataPath) == false)
						continue;

					var assetImport = AssetImporter.GetAtPath(gameDataPath);
					if (assetImport == null)
						continue;

					var gameDataSettings = GameDataSettings.Load(gameDataPath);
					var codeGenerationPath = FileUtils.MakeProjectRelative(gameDataSettings.CodeGenerationPath);
					if (gameDataSettings.Generator == (int)GameDataSettings.CodeGenerator.None)
						continue;

					var generationOptions = gameDataSettings.Options;

					var generator = (GameDataSettings.CodeGenerator)gameDataSettings.Generator;
					switch (generator)
					{
						case GameDataSettings.CodeGenerator.CSharpCodeAndAsset:
							if (!string.IsNullOrEmpty(gameDataSettings.AssetGenerationPath))
							{
								AssetGenerator.Instance.AddPath(gameDataPath);
								generationOptions &= ~(int)GameDataSettings.CodeGenerationOptions.SuppressJsonSerialization;
							}
							goto generateCSharpCode;
						case GameDataSettings.CodeGenerator.CSharp:
							generateCSharpCode:
							output
								.Append("\"").Append("../").Append(FileUtils.GetToolsPath()).Append("\"")
								.Append(" ")
								.Append(generator == GameDataSettings.CodeGenerator.CSharp ? "GENERATECSHARPCODE" : "GENERATEUNITYCSHARPCODE")
								.Append(" ")
								.Append("\"").Append("../").Append(gameDataPath).Append("\"")
								.Append(" ")
								.Append("--namespace").Append(" ").Append(gameDataSettings.Namespace)
								.Append(" ")
								.Append("--gameDataClassName").Append(" ").Append(gameDataSettings.GameDataClassName)
								.Append(" ")
								.Append("--entryClassName").Append(" ").Append(gameDataSettings.EntryClassName)
								.Append(" ")
								.Append("--options").Append(" ").Append("\"").Append(generationOptions.ToString()).Append("\"")
								.Append(" ")
								.Append("--output").Append(" ").Append("\"").Append("../").Append(codeGenerationPath).Append("\"")
								.Append(" ")
								.Append("--verbose")
								.AppendLine();
							break;
						default:
							Debug.LogError("Unknown code/asset generator type " + (GameDataSettings.CodeGenerator)gameDataSettings.Generator + ".");
							break;
					}
				}

				File.WriteAllText(RECOVERYSCRIPTS_PATH, output.ToString());
			}
			catch (Exception e)
			{
				if (!Settings.Current.Verbose)
					return;

				Debug.LogError("Failed to create recovery scripts: ");
				Debug.LogError(e);
			}
		}
	}
}
