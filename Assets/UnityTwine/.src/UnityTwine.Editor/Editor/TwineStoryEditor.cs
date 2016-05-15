using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityTwine;
using System.Collections.Generic;
using System.Runtime.Serialization;

[CustomEditor(typeof(TwineStory), true)]
public class TwineStoryEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		var story = target as TwineStory;
		if (story == null || story.Output == null)
			return;

		EditorGUILayout.Separator();

		EditorGUILayout.LabelField("Story State", story.State.ToString());

		EditorGUILayout.Separator();

		EditorGUI.indentLevel++;

		for(int i = 0; i < story.Output.Count; i++)
		{
			TwineOutput output = story.Output[i];

			if (output is TwineEmbed)
				continue;

			if (output is TwineStyleTag)
			{
				var tag = (TwineStyleTag)output;
				if (tag.TagType == TwineStyleTagType.Closer)
					EditorGUI.indentLevel--;
			}

			EditorGUILayout.LabelField(output.ToString());

			if (output is TwineStyleTag)
			{
				var tag = (TwineStyleTag)output;
				if (tag.TagType == TwineStyleTagType.Opener)
					EditorGUI.indentLevel++;
			}
		}
		EditorGUI.indentLevel--;
	}
}
