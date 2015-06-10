using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public static class ImageFade
{
	static Dictionary<Component, Coroutine> _fades = new Dictionary<Component, Coroutine>();

	public static YieldInstruction Start(Graphic img, float from, float to, float time)
	{
		Stop(img);
		var coroutine = img.StartCoroutine(AnimateGraphic(img, from, to, time));
		_fades.Add(img, coroutine);
		return coroutine;
	}

	public static YieldInstruction Start(CanvasGroup group, MonoBehaviour script, float from, float to, float time)
	{
		Stop(group, script);
		var coroutine = script.StartCoroutine(AnimateGroup(group, from, to, time));
		_fades.Add(group, coroutine);
		return coroutine;
	}

	public static void Stop(Graphic img)
	{
		Coroutine current;
		if (!_fades.TryGetValue(img, out current))
			return;

		if (current != null)
			img.StopCoroutine(current);
		_fades.Remove(img);
	}

	public static void Stop(CanvasGroup group, MonoBehaviour script)
	{
		Coroutine current;
		if (!_fades.TryGetValue(script, out current))
			return;

		script.StopCoroutine(current);
		_fades.Remove(group);
	}

	public static bool IsInProgress(Component img)
	{
		return _fades.ContainsKey(img);
	}

	static IEnumerator AnimateGraphic(Graphic img, float from, float to, float time)
	{
		Color color = img.color;
		float state = (color.a - from) / (to - from);

		for (float t = state * time; t <= time; t += Time.deltaTime)
		{
			color = img.color;
			color.a = Mathf.Lerp(from, to, t / time);
			img.color = color;
			yield return null;
		}

		color = img.color;
		color.a = to;
		img.color = color;
		_fades.Remove(img);
	}

	static IEnumerator AnimateGroup(CanvasGroup group, float from, float to, float time)
	{
		float state = (group.alpha - from) / (to - from);

		for (float t = state * time; t <= time; t += Time.deltaTime)
		{
			group.alpha = Mathf.Lerp(from, to, t / time);
			yield return null;
		}

		group.alpha = to;
		_fades.Remove(group);
	}
}