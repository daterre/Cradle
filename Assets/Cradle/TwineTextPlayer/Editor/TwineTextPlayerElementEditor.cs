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

		StoryStyle style = output.Style;
		if (style != null)
		{
			EditorGUILayout.LabelField("Style");
			EditorGUI.indentLevel++;
			foreach(string option in style.SettingNames)
			{
				List<object> values = style.GetValues(option);
				foreach(object val in values)
					EditorGUILayout.LabelField(option, val.ToString());
			}
			EditorGUI.indentLevel--;
		}
	}
}
