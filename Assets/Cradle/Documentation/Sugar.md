# Sugar - Cradle story format
A superset of both Sugarcane (Twine 1) and SugarCube (Twine 2) storyformats.

**Note**: Many of the more advanced features of SugarCube are not currently supported. SugarCube doesn't feature a proper lexer and relies heavily on JavaScript and DOM, which makes it hard to transcode to C#. It is therefore recommended to use the Harlowe format, which has a more independent syntax, for new Cradle stories.

- [Supported syntax](#supported-syntax)
	- [Links](#links)
	- [Tags](#tags)
	- [Macros](#macros)
	- [Functions](#functions)
	- [General](#general)
	

#### Supported syntax
Here is a list of what works and what doesn't:

##### Links
Links work as expected:

* Simple: `[[passage]]`
* With link text: `[[text|passage]]`
* With variable setters: `[[text|passage][$var = 123]]`
* With expressions: `[[text|either("a", "b")]]`

A syntax extension allows **naming** links for easy reference in Unity scripts:
* `[[continue = Continue down the hall.|hallway]]`

##### Variables
Global variables that start with the `$` sign are supported.
Local variables (starting with `_`) are not supported yet.
Naked variables in text will be displayed correctly: `His shirt was $shirtColor`.

Arrays are not supported out-of-the-box. This is because SugarCube uses JavaScript directly for this. To use arrays, 

##### Macros
Macros that work:

* `<<set>>`. `<<run>>`  (both single and multiple variables)
* `<<print>>`, `<<=>>`
* `<<display>>` (including Sugarcane's [shorthand](#https://twinery.org/wiki/display) syntax)
* `<<if>>` .. `<<else>>` .. `<<endif>>`
* `<<for>>` with `<<continue>>` and `<<break>>` (but the shorthand format without init and post expressions is not supported)
* `<<nobr>>`, `<<silently>>`

Macros that **don't work yet**:

* `<<unset>>`, `<<remember>>`, `<<forget>>`
* `<<goto>>`
* `<<->>`
* `<<actions>>`, `<<choice>>`
* `<<back>>`, `<<return>>`, 
* `<<click>>`, '<<button>>'
* `<<script>>`
* `<<textinput>>`, `<<radio>`, `<<radiobutton>>`, `<<checkbox>>`, `<<textarea>>`, `<<textbox>>`
* `<<addclass>>`, `<<removeclass>>`, `<<toggleclass>>`
* `<<copy>>`, `<<append>>`, `<<prepend>>`, `<<replace>>`, `<<remove>>`
* `<<repeat>`, `<<timed>>`, `<<stop>>`, `<<next>>`
* `<<widget>>`

#####Functions

Functions that work:

* `either()`
* `random()`
* `randomFloat()`
* `previous()`
* `passage()`
* `tags()` (returns a C# array, so `indexOf` and other JavaScript-specific features will not work in Unity)
* `visited()`
* `visitedTag()`
* `turns()`
* `parameter()`

Functions that **don't work yet**:

* `rot13()`
* `confirm()`, `prompt()`, `alert()`, `open()`
* `variables()`
* Native JavaScript [object methods](http://www.motoslave.net/sugarcube/2/docs/native-object-methods.html) not supported

#####General

* Strings should use `"double quotes"`, not `'single quotes'`.
* Presentation features such as Stylesheet, Script, Image and Annotation are not supported.