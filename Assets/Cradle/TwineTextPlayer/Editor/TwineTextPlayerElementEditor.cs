using UnityEngine;
using System.Collections;
using UnityEditor;
using Cradle;
using System.Collections.Generic;
using System.Runtime.Serialization;

[CustomEditor(typeof(TwineTextPlayerElement))]
public class TwineTextPlayerElementEditor : Editor {

	public static ObjectIDGenerator idGenerator = new ObjectIDGenerator();

	public override void OnInspectorGUI()
	{
		StoryOutput output = ((TwineTextPlayerElement)target).SourceOutput;

		bool unused;

		EditorGUILayout.LabelField("Type", output.GetType().Name);
		EditorGUILayout.LabelField("Id", idGenerator.GetId(output, out unused).ToString());
		EditorGUILayout.LabelField("Name", output.Name);
		EditorGUILayout.LabelField("Text", output.Text);

		EditorGUILayout.LabelField("Style");
		EditorGUI.indentLevel++;
		StoryStyle style = output.GetAppliedStyle();
		foreach(var pair in style)
			EditorGUILayout.LabelField(pair.Key, pair.Value != null ? pair.Value.ToString() : null);
		EditorGUI.indentLevel--;
		
	}
}
