using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityTwine;
using System.Collections.Generic;
using System.Runtime.Serialization;

[CustomEditor(typeof(TwineTextPlayerElement))]
public class TwineTextPlayerElementEditor : Editor {

	public static ObjectIDGenerator idGenerator = new ObjectIDGenerator();

	public override void OnInspectorGUI()
	{
		TwineOutput output = ((TwineTextPlayerElement)target).SourceOutput;

		bool unused;

		EditorGUILayout.LabelField("Type", output.GetType().Name);
		EditorGUILayout.LabelField("Id", idGenerator.GetId(output, out unused).ToString());
		EditorGUILayout.LabelField("Name", output.Name);
		EditorGUILayout.LabelField("Text", output.Text);

		TwineContext context = output.ContextInfo;
		if (context != null)
		{
			EditorGUILayout.LabelField("Context Info");
			EditorGUI.indentLevel++;
			foreach(string option in context.Options)
			{
				List<object> values = context.GetValues(option);
				foreach(object val in values)
					EditorGUILayout.LabelField(option, val.ToString());
			}
			EditorGUI.indentLevel--;
		}
	}
}
