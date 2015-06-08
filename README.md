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
(link to Snoozing)



##Documentation

###Importing
The UnityTwine asset importer listens for any new .twee files dropped into the project directory, and proceeds to import them.

The asset importer treats Twee code as logic, translating it into a C# script with a similar structure (and file name).
A story can be reimported over and over again as necessary; the C# script will be refreshed.

####Exporting .twee files from Twine
#####For Twine 1:

1. File > Export > Twee Source Code...
2. Save to a location inside your Unity project.

#####For Twine 2:
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
* With expressions: `[[text|either("a", "b")]]

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
* `tags()`

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
TwineStory includes several methods that allow other scripts to play and interact with a running story.

* `Begin()` - starts the story by playing the passage defined by StartPassage.
* `Advance(string linkName)` - simulates a 'click' on the link with the specified name (see [naming links](#naming-links)), executes its setters and jumps to the linked passage
* `Goto(string passageName)` - jumps to the specified passage and plays the story from there. (Only recommended for special cases.)

####Story state
When a story is playing, it can have one of several states. The state of the story is accessible from the TwineStory.State property.

* *Idle* - the story has either not started or has completed executing a passage. Inspect the `Output`, `Links`, and `Text` properties of the story to see what was outputted, and then call `Advance()` to continue.
* *Complete* - the story finished executing a passage, but no links were outputted, in effect ending the story.
* *Playing* - the story is currently executing a passage; interaction methods will not work.
* *Paused* - the story is currently executing a passage, but was paused in the middle; interaction methods will not work. Call `Resume()` to continue.

####Story state


