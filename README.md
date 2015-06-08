# UnityTwine
**Play Twine stories in Unity.**

1. Export .twee files from Twine 1 or 2 ([instructions](#export)) and drag into Unity
2. Add the generated script to your scene
3. Use hooks to script story interaction, or use the included TwineTextPlayer

[Twine](http://twinery.org) is an extremely simple and flexible tool for writing interactive stories. [Unity](http://unity3d.com) is a powerful and feature-rich engine for building games and interactive experiences. Both platforms are very popular and have a large user base, but could not be easily combined till now.

The goal of UnityTwine is to bring the two worlds together. Writers can independently design and test their stories as they would a normal Twine story.
Programmers and artists can develop the interaction and presentation of the game without worrying about flow control. When imported to Unity, the story bridges the gap. A story can be reimported over and over as necessary without disrupting the creative process of either side.

[Download the latest UnityTwine package](https://github.com/daterre/UnityTwine/tree/master/Assets/Plugins/UnityTwine) to begin.


#####Exporting .twee files from Twine
######Twine 1
1. File > Export > Twee Source Code...
2. Save to a location inside your Unity project.
######Twine 2
2. Export your story in .twee format from Twine 1.x, or use the [Entweedle](http://www.maximumverbosity.net/twine/Entweedle/) story format for Twine 2.x.


#####Documentation
######How it works
1. The UnityTwine asset importer translates the Twee content and logic into a C# script, preserving the structure of every passage.
2. Once added to the scene, the generated script plays each passage and provides a list of 

