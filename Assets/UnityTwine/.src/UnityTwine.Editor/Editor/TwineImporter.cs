using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using System.Reflection;

namespace UnityTwine.Editor
{
	public abstract class TwineImporter
	{
		public string AssetPath { get; internal set; }
		public StoryFormatTranscoder Transcoder {get; protected set;}
		public TwinePassageData CurrentPassage { get; private set; }

		public readonly List<TwinePassageData> Passages = new List<TwinePassageData>();
		public readonly HashSet<string> Vars = new HashSet<string>();
		public HashSet<MacroLib> MacroLibs { get; private set; }
		public Dictionary<string,MacroDef> Macros { get; private set; }
		public StoryFormatMetadata Metadata { get; protected set; }

		public virtual bool IsAssetRelevant()
		{
			return true;
		}
		
		public virtual void Initialize()
		{
		}

		public void Transcode()
		{
			if (this.Transcoder == null)
				throw new System.NotImplementedException("TwineImporter.Transcoder must be set by the importer implementation.");

			this.Metadata = this.Transcoder.GetMetadata();

			// Load macro types 
			Macros = new Dictionary<string, MacroDef>(StringComparer.OrdinalIgnoreCase);
			MacroLibs = new HashSet<MacroLib>(LoadMacrosInto(Macros, this.Metadata.StoryBaseType));

			this.Transcoder.Init();

			for (int i = 0; i < this.Passages.Count; i++)
			{
				CurrentPassage = this.Passages[i];

				CurrentPassage.Tags = Regex.Replace(CurrentPassage.Tags, @"([^\s]+)", "\"$&\",");

				try
				{
					CurrentPassage.Code = this.Transcoder.PassageToCode(CurrentPassage);
				}
				catch(TwineTranscodeException ex)
				{
					ex.Passage = CurrentPassage.Name;
					throw;
				}
			}
		}

		public void RegisterVar(string name)
		{
			Vars.Add(name);
		}

		IEnumerable<MacroLib> LoadMacrosInto(Dictionary<string, MacroDef> macros, Type storyType)
		{
			string projectDir = Directory.GetParent((Path.GetFullPath(Application.dataPath))).FullName;
			Type baseType = typeof(TwineRuntimeMacros);

			int libIndex = 0;
			foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				// Skip references external to the project
				if (!string.IsNullOrEmpty(assembly.Location) && !Path.GetFullPath(assembly.Location).StartsWith(projectDir, StringComparison.OrdinalIgnoreCase))
					continue;
				
				foreach(Type type in assembly.GetTypes())
				{
					if (type.IsAbstract || type.IsNested || !baseType.IsAssignableFrom(type))
						continue;

					// Ensure that only macro libraries for this story type are loaded
					object[] classAttributes = type.GetCustomAttributes(typeof(TwineMacroLibraryAttribute), true);
					if (classAttributes.Length > 0 && !classAttributes.Any(attr => ((TwineMacroLibraryAttribute)attr).StoryType.IsAssignableFrom(storyType)))
						continue;

					MacroLib macroLib = new MacroLib()
					{
						Type = type
					};

					bool hasMethods = false;
					foreach (MethodInfo method in type.GetMethods())
					{
						var attr = (TwineRuntimeMacroAttribute) Attribute.GetCustomAttribute(method, typeof(TwineRuntimeMacroAttribute), true);
						if (attr == null)
							continue;

						hasMethods = true;
						macros[attr.TwineName ?? method.Name] = new MacroDef()
						{
							Name = method.Name,
							Lib = macroLib
						};
					}

					if (hasMethods)
					{
						macroLib.Name = "macros" + (++libIndex).ToString();
						yield return macroLib;
					}
				}
			}
		}
	}

	public class MacroLib
	{
		public Type Type;
		public string Name;
	}

	public class MacroDef
	{
		public string Name;
		public MacroLib Lib;
	}
}