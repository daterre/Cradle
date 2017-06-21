<img src='/Documentation/cradle-logo.png?raw=true' width='250' alt="Cradle">

*Twine and Twine-like stories in Unity.*

**[Cradle](http://github.com/daterre/Cradle)** (formerly **UnityTwine**) is a plugin for [Unity](http://unity3d.com) that powers the storytelling side of a game. Based on the foundations of **[Twine](http://twinery.org)**, it imports Twine stories, plays them and makes it easy to add custom interactivity via scripting.

Writers can independently design and test their stories as they would a normal Twine story;
programmers and artists can develop the interaction and presentation without worrying about flow control. When imported to Unity, Cradle kicks in and brings the two worlds together.

#### Getting started
1. [Download the latest Cradle release](https://github.com/daterre/Cradle/releases) and install the package to your Unity project.
2. Publish a story from Twine 1 or 2 ([instructions](#exporting-from-twine)) and drag it into Unity.
3. Add the generated script to your scene.
4. Use [cues](#cues) to script story interaction, or use the included [TwineTextPlayer](#twinetextplayer).

#### Examples
[Snoozing](http://daterre.com/projects/snoozing/) is a short interactive story created with Cradle. The entire source code is [available here](http://github.com/daterre/Snoozing).

#### Contribute
Cradle is in active development. It is currently being used for the development of the puzzle-adventure game [Clastic Morning](http://daterre.com/works/clastic/), as well as other smaller projects. Hopefully it will be useful to anyone looking to create narrative-based games in Unity.

If you use Cradle in your project or game jam and find bugs, develop extra features, or even just have general feedback, please contribute by submitting to this GitHub page. Thank you!

Logo design by [Eran Hilleli](http://eranhill.tumblr.com/).

## Documentation

**Table of Contents**
- [Overview](#overview)
	- [What is Cradle?](#what-is-cradle)
	- [What is it not?](#what-is-it-not)
- [Installation](#installation)
- [Importing a story](#importing-a-story)
	- [Exporting from Twine](#exporting-from-twine)
		- [From Twine 2](#from-twine-2)
		- [From Twine 1](#from-twine-1)
	- [Supported story formats](#supported-story-formats)
- [Playback](#playback)
	- [TwineTextPlayer](#twinetextplayer)
- [Scripting](#scripting)
	- [Interacting with the story](#interacting-with-the-story)
	- [Reading story content](#reading-story-content)
	- [Links](#links)
		- [Named links](#named-links)
	- [Variables](#variables)
	- [Story state](#story-state)
		- [Pause and Resume](#pause-and-resume)
	- [Cues](#cues)
		- [Simple example](#simple-example)
		- [Setting up a cue script](#setting-up-a-cue-script)
		- [Cue types](#cue-types)
		- [Coroutine cues](#coroutine-cues)
- [Extending](#extending)
	- [Runtime macros](#runtime-macros)
	- [Variable types](#variable-types)
	- [Code generation macros](#code-generation-macros)
	- [Addition story formats](#additional-story-formats)
- [Source code](#source-code)
- [Change log](#change-log)


### Overview

#### What is Cradle?

**A framework for building a narrative game.**
A story-driven game relies on player choices to unfold, often branching out in many directions. The code required to handle and respond to these choices can be cumbersome and easily broken, getting messier and harder to maintain as the project grows. Cradle offers a clean, straightforward system for adding story-related code to a game, keeping it separate from other game code and ensuring that changes to the narrative flow and structure can be done with minimal hassle.

**An editor plugin that imports Twine stories into this framework.**
[Twine](http://twinery.org) is a popular, simple yet powerful tool for writing and publishing interactive stories. Using Twine to write the story parts of a game allows leveraging its tools and its wonderful community, with the added benefit of having a lightweight text-only version of the game that can be played and tested outside of Unity. Whenever a new version of the story is ready, it's published from Twine as an HTML file and dropped into Unity.

#### What is it not?

**It is not a Twine emulator.**
Cradle is not meant to be a Unity-based version of Twine (even though it comes pretty close with the [TwineTextPlayer](#twinetextplayer)). It is also not an embedded HTML player in Unity. Rather, it turns a Twine file into a standard Unity script which, when added to a scene, runs the story and exposes its text and links to other game scripts, which can use them creatively.

**It is not only for text and dialog.**
Twine can be an excellent interactive dialog editor, but it can do many other things as well. Cradle doesn't make any assumptions about how your story will be used or displayed in your game. You could choose to trigger a story choice when the player clicks on a certain object, or treat a specific passage as a cue to play a cutscene.

### Installation

There are 2 ways to install Cradle into a Unity project:

* Download the latest release from the [Unity Asset Store]() or from [GitHub](https://github.com/daterre/Cradle/releases) and open the package.
* Using Git, clone this repository into the Assets/Cradle folder of your project (or add it to that folder as a Git submodule if your project is a Git repo itself).

### Importing a story
The Cradle asset importer listens for any new .html or .twee files dropped into the project directory, and proceeds to import them.

The asset importer treats the Twine markup as logic, translating it into a C# script with a similar structure (and file name).

A story can be reimported over and over again as necessary; the C# script will be refreshed.

#### Exporting from Twine

##### From Twine 2
1. Choose Publish to File from the story menu.
2. Save to a location inside your Unity project.

##### From Twine 1
1. File > Export > Twee Source Code...
2. Save to a location inside your Unity project.

#### Supported story formats
Cradle supports the following Twine story formats:
* **[Harlowe](http://twine2.neocities.org/)**, the default format of Twine 2 (recommended)
* **[Sugarcane](https://twinery.org/wiki/twine_reference)**, the default format of Twine 1
* **[SugarCube](http://www.motoslave.net/sugarcube/)**, a richer version of Sugarcane that works in both Twine 1 and 2

Most features of these story formats are available in Cradle, but there are some limitations. Please see their individual readme's in the [Documention](Assets/Cradle/Documentation) folder for information on supported macros, syntax, and more.

Cradle can be extended to support additional story formats (Twine or other), see [Extending](#extending).


### Playback
Once a story is imported and a story script is generated, this script can be added to a game object in the scene like any normal script.

All story scripts include the following editor properties:

* `AutoPlay`: when true, begins playing the story immediately when the game starts.
* `StartPassage`: indicates which passasge to start playback from
* `AdditionalCues`: additional game objects on which to search for cues (see the [cues](#cues) section for more info)
* `OutputStyleTags`: when checked, the story will output tags that indicate style information (see the [styles](#styles) section for more info)

#### TwineTextPlayer
Included in Cradle is a prefab and script that can be used to quickly display and interact with a story in a text-only fashion. The prefab is built with Unity UI components (4.6+) to handle text and layout. **To use:**

1. Create a new scene
2. Drag the TwineTextPlayer prefab into the scene
3. Import your Twine story and drag the generated C# script onto the TwineTextPlayer object
4. Play your scene

### Scripting
Each passage in an imported story becomes a function that outputs text or links. Custom scripts can listen for generated output,
displaying it as necessary and controlling which links are used to advance the story.

To understand scripting with Cradle it is first necessary to get to know the `Story` class, from which all imported stories derive.

#### Interacting with the story
The `Story` class is at the heart of Cradle. It contains the story content and includes several methods that allow other scripts to play and interact with a running story.

* `Begin()` - starts the story by playing the passage defined by StartPassage.
* `DoLink(string linkName)` - follows the link with the specified name (see [links](#links)).
* `GoTo(string passageName)` - jumps to the specified passage and plays the story from there. (Only recommended for special cases.)

Example:

```c#
public Story story;

void Start() {
	story.Begin();
}

void Update() {
	// You'd want your script to check a few things before doing this, but hey this is an example
	if (Input.GetMouseButtonDown(0))
		story.DoLink("myLink");
}

```

#### Reading story content
After a passage has been reached, its output can be inspected on your `Story` script.

* `Output` - a list of all the output of the current passage.
* `GetCurrentText()` - a sub-list of Output, includes only the text of the passage.
* `GetCurrentLinks()` - a sub-list of Output, includes only the links of the passage.
* `Tags` - the tags of the current passage
* `Vars` - the current values of any global story variables
* `CurrentPassageName` - the name of the current passage that was just executed.
* `PassageHistory` - a list of all passage names visited since the story began, in chronological order. (Passages will appear twice if visited twice.)

Passage output can also be intercepted while it is executing using [cues](#cues) or with the `OnOutput` event:

```c#
public Story story;

void Start() {
	story.OnOutput += story_OnOutput;
	story.Begin();
}

void story_OnOutput(StoryOutput output) {
	// Do something with the output here
	Debug.Log(output.Text);
}

```

#### Links
As a web-based format, Twine is built around the concept of links. Clicking on links is the primary way Twine games are played and the way a story is advanced.

In Cradle, links are represented by the `StoryLink` class, and perform either one or both of the following functions when triggered with `Story.DoLink`:
* Go to a different passage
* Execute an 'action' - a fragment of a passage which wasn't shown when the passage was entered (Example: setting variable values, revealing additional text)

If both an action and a passage name are specified, the action is executed first, and only when it is done does the story advance to the next passage.

##### Named links
Consider the following Twine link:
```
[[Visit your grandmother|grandma]]
```

To activate it and enter the "grandma" passage you must call `Story.DoLink("Visit your grandmother")` in your script. But what if the writer decides to change the text of the link to "Go to your grandmother's house"? You will have to update your script in Unity or the call to DoLink will fail. To avoid breaking links in this way, Cradle extends the standard link syntax to allow naming the link:
```
[[visitGrandma = Visit your grandmother|grandma]]
```
Now you can call `Story.DoLink("visitGrandma")` and it will work. As long as the writer keeps the name intact, changing the rest of the text will not affect scripting.

Why not just use the target passage as the link's name, you ask? For two reasons:

1. Links don't necessarily have a passage, they could just execute an action
2. Two or more simultaneous links could point to the same passage, but have different actions

#### Variables
Stories often use macros to store values in variables, reading them later in order to check conditions (`if`), display them (`print`) and more. In scripting, these variables can be accessed in two ways:

Using the getter or setter. Example:
```c#
public Story story;

void Update() {
	if (story.Vars["ammo"] > 10) {
		Debug.Log("Ammo limited to 10");
		story.Vars["ammo"] = 10;
	}
}
```

Using the generated variable directly:
```c#
public JimsAdventure story; // generated class name from the file JimsAdventure.twee

void Update() {
	if (story.Vars.ammo > 10) {
		Debug.Log("Ammo limited to 10");
		story.Vars.ammo = 10;
	}
}
```

Notes:

* Variables are all of type StoryVar, which is a dynamic value type that can represent a string, a number, a boolean or any other type supported by the story format in use.  

#### Story state
When a story is playing, it can have one of several states. The state of the story is accessible from the Story.State property.

* `Idle` - the story has either not started or has completed executing a passage or a passage fragment. Inspect the `Output` property of the story to see what was outputted, and then call `DoLink()` to continue.
* `Playing` - the story is currently executing a passage; interaction methods will not work.
* `Paused` - the story is currently executing a passage, but was paused in the middle; interaction methods will not work. Call `Resume()` to continue.

To detect when the state has changed, use the `OnStateChanged` event:
```c#
public Story story;

void Start() {
	story.OnStateChanged += story_OnStateChanged;
	story.Begin();
}

void story_OnStateChanged() {
	if (story.State == StoryState.Idle) {
		// Interaction methods can be called now
		story.DoLink("enterTheCastle");
	}
}
```

##### Pause and Resume
The story can be paused in order to do time-consuming tasks such as waiting for animations to end or for a scene to load, before further story output is generated. Pausing is only necessary when the story is in the Playing state; if it is Idle, there is nothing to pause.

Example (using [cues](#cues)):
```c#
publicStory story;
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


#### Cues
Cradle includes a powerful cue system that allows scripts to easily run in conjuction with the current passage.

**Note**: Before version 2.0 cues were called 'hooks', this was changed to avoid confusion with the term hook as it is used in the Harlowe story format.

##### Simple example

Let's say your story includes 2 passages named "Attack" and "Defend". Here's a script with cues to change camera background color according to the passage.

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

##### Setting up a cue script

1. Create a new script (recommended C#, but UnityScript should work)
2. Either add it to the same game object that contains your story script, or...
3. ...add it to any game object in your scene and add that game object to the AdditionalCues list


##### Cue types
The following cues types are supported (replace 'passage' with the name of a passage):

* `passage_Enter()` - called immediately when a passage is entered. This means after Begin, DoLink or GoTo are called  and whenever a sub-passage is embedded via a macro (i.e. Twine's `display` macro)
* `passage_Exit()` - called on the current passages just before a new main passage is entered via DoLink or GoTo. (An embedded sub-passage's exit cue is called before that of the passage which embedded it, in a last-in-first-out order.)
* `passage_Done()` - called when the passage is done executing and the story has entered the Idle state. All [passage output](#reading-story-content) is available.
* `passage_link_Done()` - called after a link's action has been completed, and before the next passage is entered (if specified). (replace 'link' with the name of a link)
* `passage_Update()` - when the story is in the Idle state, this cue is called once per frame.
* `passage_Output(StoryOutput output)` - whenever a passage generates output (text, links, etc.), this cue receives it. 

If you want to attach a cue to a passage with a name that contains spaces or other characters not allowed in C#, you can decorate your method with an attribute:

```C#
using Cradle;

// Specifies that this method is an Enter cue for the passage named "A large empty yard"
[StoryCue("A large empty yard", "Enter")]
void enterYardCutscene()
{
	// ...
}
```

Notes:
* You can have multiple StoryCue attributes on a single method.
* The StoryCue attribute takes precedence over the method's name, so if an attribute is present the method's name is ignored, even if it looks like a valid cue name.

##### Coroutine cues
If a cue is an enumeration method (returns `IEnumerator` in C# or includes a `yield` statement in UnityScript) it is used to start a coroutine. Coroutine cues behave just like normal Unity coroutines.

```c#
IEnumerator spaceship_Enter() {
	Debug.Log("Wait for it...");
	yield return new WaitForSeconds(3f);
	Debug.Log("Go!")
}
```

Notes:

* All cues can be coroutines except the Update cues, which must always return void.
* After the first yield, the story will be Idle and all the the passage output will be available. This is because the passage continues execution after calling the cue, so by the time the coroutine has done waiting, the passage is complete. To pause execution until the coroutine ends, use `Pause()` and `Resume()` ([example](#pause-and-resume))


### Extending
Cradle can be extended to include macros and var types that do not exist within the original story format.

#### Runtime macros
Runtime macros are the simplest kind of extension to add to Cradle. A runtime macro is simply a function that you can call from within a story passage. It can't generate any additional story output or affect the flow of passages, but it can trigger some Unity-specific functionality at precise points in your story.

1. Create a normal C# script (i.e. not in any of the Editor folders)
2. Instead of `MonoBehaviour`, your class should inherit from `Cradle.RuntimeMacros`
3. To expose a method as a runtime macro, simply decorate it with the `[RuntimeMacro]` attribute. If you want the name of the macro as written in Twine to be different from the C# method name, simply add the name to the attribute: `[RuntimeMacro("sfx")]`
4. Import your story.

Here is a complete example that plays/stops an audio source:

```c#
using UnityEngine;

public SoundEffectsMacros: Cradle.RuntimeMacros
{
	[RuntimeMacro]
	public void sfxPlay(string soundName)
	{
		GameObject.Find(soundName).GetComponent<AudioSource>().Play();
	}

	[RuntimeMacro]
	public void sfxStop(string soundName)
	{
		GameObject.Find(soundName).GetComponent<AudioSource>().Stop();
	}
}
```

Here's how to use it in Harlowe:
```
Gareth stares intently at the screen and presses the play button.
(sfx-stop: "ambient")
(sfx-play: "recording")
```

Notes:
* To access the Story component from within a macro, simple use `this.Story`
* If you want to add properties that can be assigned from the editor, it is recommended to pass the call onto a regular MonoBehaviour script attached to the same GameObject as your Story component. For example, `this.Story.SendMessage("PlaySound", soundName);` will pass the macro onto any script attached to that GameObject, where properties can be defined/assigned and the actual work can be done.
* An instance of this class is created once per story. So any member variables will exist for the lifetime of your Story component.
* When played in the browser, the Sugarcane/Cube story formats might throw an error if an unrecognized function is encountered. The easiest way to avoid this is to create a custom dummy JavaScript function that will avoid the error. Example (add this in your story's JavaScript):

```js
window.sfxPlay = function() {};
window.sfxStop = function() {};
```

#### Variable types
(TODO)

#### Code generation macros
(TODO)

#### Additional story formats
(TODO)

### Source code
The plugin source code is available in the .src directory on GitHub (the period prefix hides this folder from Unity). There are separate solutions for Visual Studio and MonoDevelop. To build, open the appropriate solution for your IDE and run "Build Solution" (Visual Studio) or "Build All" (MonoDevelop). The DLLs created will replace the previous DLLs in the the Cradle plugin directory.

If you make modifications to the source code, you might want to run the [Cradle test suite](http://github.com/daterre/Cradle-Testing).

### Change log

##### Version 2.0.1
* Renamed the project to Cradle
* Support for published HTML files, including Twine 2
* Story formats: added Harlowe, improved Sugarcane/Sugarcube
* Complete rewrite of the asset importer:
	*  Extensible, supports multiple story formats
	*  Error checking and graceful failures that don't break the project
* Complete rewrite of the variable system to allow support for many different types (arrays, datasets, etc.)
* Added support for passage fragments to allow deferred code execution and output generation (Harlowe makes extensive use of this feature)
* Source code now compiles to standlone assemblies

##### Version 1.1
* First full release (though not published on Asset Store)
* Parser now supports any valid Twine passage name
* Added extensible macro system
* Shorthand display syntax support (with parameters)
* Visited, visitedTag, turns, parameter functions
* Tags are now simple string arrays like the original Twine
* Simplified hook setup

##### Version 1.0 beta 1
Pre-release.