# UnityTwine
**Play Twine stories in Unity.**

[Twine](http://twinery.org) is an extremely simple and flexible tool for writing interactive stories. [Unity](http://unity3d.com) is a powerful and feature-rich engine for building games and interactive experiences. [UnityTwine](http://github.com/daterre/UnityTwine) brings the two worlds together.

Writers can independently design and test their stories as they would a normal Twine story;
programmers and artists can develop the interaction and presentation without worrying about flow control. When imported to Unity, UnityTwine kicks in and bridges the gap. 

####Getting started
1. [Download the latest UnityTwine release](https://github.com/daterre/UnityTwine/tree/master/Assets/Plugins/UnityTwine).
2. Export .twee files from Twine 1 or 2 ([instructions](#exporting-twee-files-from-twine)) and drag into Unity
3. Add the generated script to your scene
4. Use [hooks](#hooks) to script story interaction, or use the included [TwineTextPlayer](#twinetextplayer)

####Examples
[Snoozing](http://daterre.com/projects/snoozing) is a small game created with UnityTwine. The entire source code is included here on GitHub and in the Unity asset store bundle.

####Contribute
UnityTwine is in active development. It is currently being used for the development of the puzzle-adventure game [Clastic Morning](http://daterre.com/works/clastic/), as well as other smaller projects. Hopefully it will be useful to anyone looking to create narrative-based games in Unity.

If you use UnityTwine in your project or game jam and find bugs, develop extra features, or even just have general feedback, please contribute by submitting to this GitHub page. Thank you!



##Documentation

**Table of contents**
- [Importing](#)
	- [Exporting .twee files from Twine](#)
		- [For Twine 1](#)
		- [For Twine 2](#)
	- [Supported syntax](#)
		- [Links](#)
		- [Tags](#)
		- [Macros](#)
		- [Functions](#)
		- [General](#)
- [Playback](#)
	- [TwineTextPlayer](#)
- [Scripting](#)
	- [Story interaction](#)
	- [Passage output](#)
	- [Variables](#)
	- [Story state](#)
		- [Pause and Resume](#)
	- [Hooks](#)
		- [Simple example](#)
		- [Setting up a hook script](#)
		- [Hook types](#)
		- [Coroutine hooks](#)

###Importing
The UnityTwine asset importer listens for any new .twee files dropped into the project directory, and proceeds to import them.

The asset importer treats Twee code as logic, translating it into a C# script with a similar structure (and file name).
A story can be reimported over and over again as necessary; the C# script will be refreshed.

####Exporting .twee files from Twine
#####For Twine 1

1. File > Export > Twee Source Code...
2. Save to a location inside your Unity project.

#####For Twine 2
Requires the [Entweedle](http://www.maximumverbosity.net/twine/Entweedle/) story format.

1. Click the "Formats" link in Twine, then "Add a New Format" and enter this URL: `http://www.maximumverbosity.net/twine/Entweedle/format.js`
2. In your story, select Entweedle as your story format.
3. Press 'Play'
4. Copy and paste the resulting text into an empty text file with the .twee extension. Put this file in your Unity project.

####Supported syntax
UnityTwine supports many Twine features, but not all (yet). Here is a list of what works and what doesn't:

#####Links
Links work as expected:

* Simple: `[[passage]]`
* With link text: `[[text|passage]]`
* With variable setters: `[[text|passage][$var = 123]]`
* With expressions: `[[text|either("a", "b")]]`

A syntax extension allows **naming** links for easy reference in Unity scripts:
* `[[continue = Continue down the hall.|hallway]]`

#####Tags
Tags (space-delimted) work as expected, with a syntax extention allowing key-value style tags, e.g. `location:exterior weather:rainy`

#####Macros
Macros that work:

* `<<if>>` .. `<<else>>` .. `<<endif>>`
* `<<display>>` (but **not** shorthand form)
* `<<print>>`
* `<<set>>` (both single and multiple variables)

Macros that **don't work yet**:

* `<<remember>>`
* `<<action>>`
* `<<choice>>`
* `<<nobr>>`
* HTML stuff: `<<textinput>>`, `<<radio>`,`<<checkbox>>`, `<<button>>`

#####Functions

Functions that work:

* `either()`
* `random()`
* `previous()`
* `passage()`
* `tags()` (returns a C# array, so `indexOf` and other JavaScript-specific features will not work in Unity)

Functions that **don't work yet**:

* `visited()`
* `visitedTag()`
* `turns()`
* `parameter()`
* `rot13()`
* HTML stuff: `confirm()`, `prompt()`, `alert()`, `open()`

#####General

* Strings should use `"double quotes"`, not `'single quotes'`.
* For usage with hooks, passage names should not begin with a number or include non-alphanumeric characters.
* Twine 1 presentation features such as Stylesheet, Script, Image and Annotation are not supported.



###Playback
Once a story is imported and a story script is generated, this script can be added to a game object in the scene like any normal script.

All story scripts include the following editor properties:

* `AutoPlay`: when true, begins playing the story immediately when the game starts.
* `StartPassage`: indicates which passasge to start playback from (default is `Start`)
* `HookScript`: a Unity script which provides story hooks (see the [hooks](#hooks) section for more info)
* A list of Twine variables used in the story, useful for debugging purposes.

####TwineTextPlayer
Included in UnityTwine is a prefab and script that can be used to quickly display and interact with a story in a text-only fashion. The prefab is built with Unity UI components (4.6+) to handle text and layout. **To use:**

1. Create a new scene
2. Create an empty game object, call it 'TwineStory'
3. Import your Twine story and drag the generated C# script onto the 'TwineStory' game object
4. Drag the TwineTextPlayer prefab into the scene
5. On the TwineTextPlayer object, drag the 'TwineStory' game object into the 'Story' proprety
6. Play your scene



###Scripting
Each passage in an imported Twine story becomes a function that outputs text or links. Custom scripts can listen for generated output,
displaying it as necessary and controlling which links are used to advance the story.

To understand scripting with UnityTwine it is first necessary to get to know the `TwineStory` class, from which all imported stories derive.

####Story interaction
The `TwineStory` class includes several methods that allow other scripts to play and interact with a running story.

* `Begin()` - starts the story by playing the passage defined by StartPassage.
* `Advance(string linkName)` - follows the link with the specified name (see [naming links](#links)): executes the setters, and then jumps to the linked passage
* `Goto(string passageName)` - jumps to the specified passage and plays the story from there. (Only recommended for special cases.)

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
* `Tags` - a merged list of all tags encountered during passage execution, including the tags of any sub-passages referenced by `<<display>>`.
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
Before you can get your hooks to work you have to tell the TwineStory where to look for them. (This step is required at the moment but will hopefully be removed in the next version.)

1. Create a new script (recommended C#, but UnityScript should work)
2. Add it to any game object in your scene
3. Drag the object onto your TwineStory component's `HookScript` property


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
