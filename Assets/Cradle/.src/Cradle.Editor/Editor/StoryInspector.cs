using UnityEngine;
using System.Collections;
using UnityEditor;
using Cradle;
using System.Collections.Generic;
using System.Runtime.Serialization;

[CustomEditor(typeof(Story), true)]
public class StoryInspector : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		var story = target as Story;
		if (story == null || story.Output == null)
			return;

		EditorGUILayout.Separator();

		EditorGUILayout.LabelField("Story State", story.State.ToString());
		EditorGUILayout.LabelField("Current Passage", story.CurrentPassageName);

		EditorGUILayout.Separator();

		int defaultIndent = EditorGUI.indentLevel;

		for(int i = 0; i < story.Output.Count; i++)
		{
			StoryOutput output = story.Output[i];

			if (output is Embed)
				continue;

			int groupCount = 0;
			OutputGroup group = output.Group;
			while (group != null)
			{
				groupCount++;
				group = group.Group;
			}
			EditorGUI.indentLevel = defaultIndent + groupCount;
			EditorGUILayout.LabelField(output.ToString());
		}
		
		EditorGUI.indentLevel = defaultIndent;
	}
}
