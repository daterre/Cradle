# UnityTwine
**Play Twine stories in Unity.**

UnityTwine is a plugin for Unity that imports Twine stories, plays them and makes it easy to add custom interactivity via scripting.

[Twine](http://twinery.org) is an extremely simple and flexible tool for writing interactive stories. [Unity](http://unity3d.com) is a powerful and feature-rich engine for building games and interactive experiences. [UnityTwine](http://github.com/daterre/UnityTwine) brings the two worlds together.

Writers can independently design and test their stories as they would a normal Twine story;
programmers and artists can develop the interaction and presentation without worrying about flow control. When imported to Unity, UnityTwine kicks in and bridges the gap. 

#### Getting started
1. [Download the latest UnityTwine release](https://github.com/daterre/UnityTwine/releases) and install the package to your Unity project.
2. Publish a story from Twine 1 or 2 ([instructions](#exporting-from-twine)) and drag it into Unity.
3. Add the generated script to your scene.
4. Use [cues](#cues) to script story interaction, or use the included [TwineTextPlayer](#twinetextplayer).

#### Examples
[Snoozing](http://daterre.com/projects/snoozing/) is a short interactive story created with UnityTwine. The entire source code is included here on GitHub (in the Examples folder) and in the Unity asset store package.

#### Contribute
UnityTwine is in active development. It is currently being used for the development of the puzzle-adventure game [Clastic Morning](http://daterre.com/works/clastic/), as well as other smaller projects. Hopefully it will be useful to anyone looking to create narrative-based games in Unity.

If you use UnityTwine in your project or game jam and find bugs, develop extra features, or even just have general feedback, please contribute by submitting to this GitHub page. Thank you!



## Documentation

**Table of Contents**
- [Overview](#overview)
- [Installation](#installation)
- [Importing a story](#importing-a-story)
	- [Supported story formats](#supported-story-formats)
	- [Exporting from Twine](#exporting-from-twine)
		- [From Twine 2](#exporting-from-twine-2)
		- [From Twine 1](#exporting-from-twine-1)
- [Playback](#playback)
	- [TwineTextPlayer](#twinetextplayer)
- [Scripting](#scripting)
	- [Story interaction](#story-interaction)
	- [Passage output](#passage-output)
	- [Variables](#variables)
	- [Story state](#story-state)
		- [Pause and Resume](#pause-and-resume)
	- [Cues](#cues)
		- [Simple example](#simple-example)
		- [Setting up a cue script](#setting-up-a-cue-script)
		- [Cue types](#cue-types)
		- [Coroutine cues](#coroutine-cues)
	- [Extending](#extending)
		- [Runtime macros]
		- [Code generation macros]
- [Change log](#change-log)


### Overview

#### What is UnityTwine?

**A framework for building a story-driven game.**
A story-driven game relies on player choices to unfold, often branching out in many directions. The code required to handle and respond to these choices can be cumbersome and easily broken, getting messier and harder to maintain as the project grows. UnityTwine offers a clean, straightforward system for adding story-related code to a game, keeping it separate from other game code and ensuring that changes to the story flow and structure can be done with minimal hassle.

**An editor plugin that imports Twine stories into this framework.**
[Twine](http://twinery.org) is a popular, simple yet powerful tool for writing and publishing interactive stories. Using Twine to write the story parts of a game allows leveraging its tools and its wonderful community, with the added benefit of having a lightweight text-only version of the game that can be played and tested outside of Unity. Whenever a new version of the story is ready, it's published from Twine as an HTML file and dropped into Unity.

#### What is it not?

**It is not a Twine emulator.**
UnityTwine is not meant to be a Unity-based version of Twine (even though it comes pretty close with the [TwineTextPlayer](#twinetextplayer)). It is also not an embedded HTML player in Unity. Rather, it turns a Twine file into a standard Unity script which, when added to a scene, runs the story and exposes its text and links to other game scripts, which can use them creatively.

**It is not a dailog editor.**
Twine can be an excellent interactive dialog editor, but it can do many other things as well. UnityTwine doesn't make any assumptions about how your story will be used or displayed in your game. You could choose to trigger a story choice when the player clicks on a certain object, or treat a specific passage as a cue to play a cutscene.

### Installation

Download the latest release from the [Unity asset store] or from [GitHub](https://github.com/daterre/UnityTwine/releases) and open the package.

### Importing a story
The UnityTwine asset importer listens for any new .html or .twee files dropped into the project directory, and proceeds to import them.

The asset importer treats the Twine markup as logic, translating it into a C# script with a similar structure (and file name).

A story can be reimported over and over again as necessary; the C# script will be refreshed.

#### Exporting from Twine

##### From Twine 1
1. File > Export > Twee Source Code...
2. Save to a location inside your Unity project.

##### From Twine 2
1. Choose Publish to File from the story menu.
2. Save to a location inside your Unity project.

#### Supported story formats
UnityTwine supports the following Twine story formats:
* **[Harlowe](http://twine2.neocities.org/)**, the default format of Twine 2 (recommended)
* **[Sugarcane](https://twinery.org/wiki/twine_reference)**, the default format of Twine 1
* **[SugarCube](http://www.motoslave.net/sugarcube/)**, a richer version of Sugarcane that works in both Twine 1 and 2

Most features of these story formats are available in UnityTwine, but there are some limitations. Please see the [Documention]() for information on supported macros, syntax, etc.


### Playback
Once a story is imported and a story script is generated, this script can be added to a game object in the scene like any normal script.

All story scripts include the following editor properties:

* `AutoPlay`: when true, begins playing the story immediately when the game starts.
* `StartPassage`: indicates which passasge to start playback from (default is `Start`)
* `AdditionalHooks`: additional game objects on which to search for hooks (see the [hooks](#hooks) section for more info)
* A list of Twine variables used in the story, useful for debugging purposes.

####TwineTextPlayer
Included in UnityTwine is a prefab and script that can be used to quickly display and interact with a story in a text-only fashion. The prefab is built with Unity UI components (4.6+) to handle text and layout. **To use:**

1. Create a new scene
2. Drag the TwineTextPlayer prefab into the scene
3. Import your Twine story and drag the generated C# script onto the TwineTextPlayer object
4. Play your scene



###Scripting
Each passage in an imported Twine story becomes a function that outputs text or links. Custom scripts can listen for generated output,
displaying it as necessary and controlling which links are used to advance the story.

To understand scripting with UnityTwine it is first necessary to get to know the `TwineStory` class, from which all imported stories derive.

####Story interaction
The `TwineStory` class includes several methods that allow other scripts to play and interact with a running story.

* `Begin()` - starts the story by playing the passage defined by StartPassage.
* `Advance(string linkName)` - follows the link with the specified name (see [naming links](#links)): executes the setters, and then jumps to the linked passage
* `GoTo(string passageName)` - jumps to the specified passage and plays the story from there. (Only recommended for special cases.)

Example:

```c#
public TwineStory story;

void Start() {
	story.Begin();
}

void Update() {
	// You'd want your script to check a few things before doing this, but hey this is an example
	if (Input.GetMouseButtonDown(0))
		story.Advance("myLink");
}

```

####Passage output
When a passage has executed, its output can be inspected on your `TwineStory` script.

* `Output` - a list of all the output of the passage, in the order in which it was generated. Includes any output from sub-passages referenced by `<<display>>`, along with the definition of those passages.
* `Text` - a sub-list of Output, includes only the text of the passage.
* `Links` - a sub-list of Output, includes only the links of the passage.
* `Tags` - the tags of the current main passage (but not of the sub-passages)
* `CurrentPassageName` - the name of the current main passage (i.e. not sub-passage) that was executed.
* `PreviousPassageName` - the name of the previous main passage (i.e. not sub-passage) that was executed.

Passage output can also be intercepted while it is executing using [hooks](#hooks) or with the `OnOutput` event:

```c#
public TwineStory story;

void Start() {
	story.OnOutput += story_OnOutput;
	story.Begin();
}

void story_OnOutput(TwineOutput output) {
	// Do something with the output here
	Debug.Log(output.Text);
}

```

####Variables
Twine stories often use `<<set $ammo = 1>>` macros to store variables, reading them later in `<<if>`, `<<print>>` or `<<display>>` macros. When imported, these variables can be accessed in two ways:

Using the getter or setter. Example:
```c#
public TwineStory story;

void Update() {
	if (story["ammo"] > 10) {
		Debug.Log("Ammo limited to 10");
		story["ammo"] = 10;
	}
}
```

Using the generated variable directly:
```c#
public JimsAdventure story; // generated class name from the file JimsAdventure.twee

void Update() {
	if (story.ammo > 10) {
		Debug.Log("Ammo limited to 10");
		story.ammo = 10;
	}
}
```

Notes:

* Variables are all of type TwineVar, which is a dynamic value type that implicitly converts itself from string to integer to double to boolean.

####Story state
When a story is playing, it can have one of several states. The state of the story is accessible from the TwineStory.State property.

* `Idle` - the story has either not started or has completed executing a passage. Inspect the `Output`, `Links`, and `Text` properties of the story to see what was outputted, and then call `Advance()` to continue.
* `Complete` - the story finished executing a passage, but no links were outputted, in effect ending the story.
* `Playing` - the story is currently executing a passage; interaction methods will not work.
* `Paused` - the story is currently executing a passage, but was paused in the middle; interaction methods will not work. Call `Resume()` to continue.

To detect when the state has changed, use the `OnStateChanged` event:
```c#
public TwineStory story;

void Start() {
	story.OnStateChanged += story_OnStateChanged;
	story.Begin();
}

void story_OnStateChanged() {
	if (story.State == TwineStoryState.Idle) {
		// Interaction methods can be called now
		story.Advance("enterTheCastle");
	}
}
```

#####Pause and Resume
The story can be paused in order to do time-consuming tasks such as waiting for animations to end or for a scene to load, before further story output is generated. Pausing is only necessary when the story is in the Playing state; if it is Idle or Complete, there is nothing to pause.

Example (using [hooks](#hooks)):
```c#
public TwineStory story;
public Sprite blackOverlay;

const float fadeInTime = 2f;

IEnumerator castle_Enter() {
	story.Pause();

	blackOverlay.color = new Color(0f, 0f, 0f, 1f);

	for (float t = 0; t <= fadeInTime; t+=Time.deltaTime) {
		// Update the alpha of the sprite
		float alpha = 1f - Mathf.Clamp(t/fadeInTime, 0f, 1f);
		blackOverlay.color = new Color(0f, 0f, 0f, alpha);

		// Wait a frame
		yield return null;
	}

	story.Resume();
}
```


####Hooks
UnityTwine includes a powerful hook system that allows scripts to easily run in conjuction with the current passage.

#####Simple example

Let's say your story includes 2 passages named "Attack" and "Defend". Here's a script with hooks to change camera background color according to the passage.

```c#
bool shieldsUp;

void Attack_Enter()
{
	Camera.main.backgroundColor = Color.blue;
}

void Defend_Enter()
{
	Camera.main.backgroundColor = Color.red;
	shieldsUp = true;
}

void Defend_Update()
{
	// Runs every frame like a normal Update method,
	// but only when the current passage is Defend
}

void Defend_Exit()
{
	shieldsUp = false;
}
```

#####Setting up a hook script

1. Create a new script (recommended C#, but UnityScript should work)

2. Either add it to the same game object that contains your story script, or...
3. ...add it to any game object in your scene and add that game object to the AdditionalHooks list


#####Hook types
The following hook types are supported (replace 'passage' with the name of a passage):

* `passage_Enter()` - called immediately when a passage is entered. This means after Begin, Advance or GoTo are called (for the main passage) and whenever a `<<display>>` macro is encountered (for sub-passages).
* `passage_Exit()` - called on the current passages just before a new main passage is entered via Advance or GoTo. (A sub-passage's exit hook is called before the passage's that referenced it, in a last-in-first-out order.)
* `passage_Done()` - called when the passage is done executing and the story has entered the Idle state. All [passage output](#passage-output) is available.
* `passage_Update()` - when the story is in the Idle state, this hook is called once per frame.
* `passage_Output(TwineOutput output)` - whenever a passage generates output (text, link, or sub-passage), this hook receives it. 


#####Coroutine hooks
If a hook is an enumeration function (returns `IEnumerator` in C# or includes a `yield` statement in UnityScript) it is used to start a coroutine. Coroutine hooks behave just like normal Unity coroutines.

```c#
IEnumerator spaceship_Enter() {
	Debug.Log("Wait for it...");
	yield return new WaitForSeconds(3f);
	Debug.Log("Go!")
}
```

Notes:

* All hooks can be coroutines except the Update hook, which must always return void.
* After the first yield, the story will be Idle and all the the passage output will be available. This is because the passage continues execution after calling the hook, so by the time the coroutine has done waiting, the passage is complete. To pause execution until the coroutine ends, use `Pause()` and `Resume()` ([example](#pause-and-resume))


###Change log

#####Version 1.1
First full release:

* Parser now supports any valid Twine passage name
* Added extensible macro system
* Shorthand display syntax support (with parameters)
* Visited, visitedTag, turns, parameter functions
* Tags are now simple string arrays like the original Twine
* Simplified hook setup

#####Version 1.0 beta 1
Pre-release.