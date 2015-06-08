# UnityTwine
**Play Twine stories in Unity.**

[Twine](http://twinery.org) is an extremely simple and flexible tool for writing interactive stories. [Unity](http://unity3d.com) is a powerful and feature-rich engine for building games and interactive experiences. [UnityTwine](http://github.com/daterre/UnityTwine) brings the two worlds together.

Writers can independently design and test their stories as they would a normal Twine story;
programmers and artists can develop the interaction and presentation without worrying about flow control. When imported to Unity, UnityTwine kicks in and bridges the gap. 

####Getting started
1. [Download the latest UnityTwine release](https://github.com/daterre/UnityTwine/tree/master/Assets/Plugins/UnityTwine).
2. Export .twee files from Twine 1 or 2 ([instructions](#exporting-twee-files-from-twine)) and drag into Unity
3. Add the generated script to your scene
4. Use [hooks](#hooks) to script story interaction, or use the included [TwineTextPlayer](#textplayer)

#####Exporting .twee files from Twine
**Twine 1:**
1. File > Export > Twee Source Code...
2. Save to a location inside your Unity project.

**Twine 2:**
Requires the [Entweedle](http://www.maximumverbosity.net/twine/Entweedle/) story format.

1. Click the "Formats" link in Twine, then "Add a New Format" and enter this URL: `http://www.maximumverbosity.net/twine/Entweedle/format.js`
2. In your story, select Entweedle as your story format.
3. Press 'Play'
4. Copy and paste the resulting text into an empty text file with the .twee extension. Put this file in your Unity project.

###Documentation
#####How it works
1. The UnityTwine asset importer translates the Twee content and logic into a C# script, preserving the structure of every passage.
2. Once added to the scene, the generated script plays each passage and provides a list of 

A story can be reimported over and over as necessary without disrupting the creative process of either side.



