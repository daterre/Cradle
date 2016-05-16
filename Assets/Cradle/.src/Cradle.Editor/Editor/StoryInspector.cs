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

		EditorGUILayout.Separator();

		EditorGUI.indentLevel++;

		for(int i = 0; i < story.Output.Count; i++)
		{
			StoryOutput output = story.Output[i];

			if (output is Embed)
				continue;

			if (output is StyleTag)
			{
				var tag = (StyleTag)output;
				if (tag.TagType == StyleTagType.Closer)
					EditorGUI.indentLevel--;
			}

			EditorGUILayout.LabelField(output.ToString());

			if (output is StyleTag)
			{
				var tag = (StyleTag)output;
				if (tag.TagType == StyleTagType.Opener)
					EditorGUI.indentLevel++;
			}
		}
		EditorGUI.indentLevel--;
	}
}
