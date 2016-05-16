
- [Supported syntax](#supported-syntax)
			- [Links](#links)
			- [Tags](#tags)
			- [Macros](#macros)
			- [Functions](#functions)
			- [General](#general)
			- 
#### Supported syntax
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
Tags (space-delimted) work as expected.

#####Macros
Macros that work:

* `<<if>>` .. `<<else>>` .. `<<endif>>`
* `<<display>>` including shorthand syntax
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
* `visited()`
* `visitedTag()`
* `turns()`
* `parameter()`

Functions that **don't work yet**:

* `rot13()`
* HTML stuff: `confirm()`, `prompt()`, `alert()`, `open()`

#####General

* Strings should use `"double quotes"`, not `'single quotes'`.
* For usage with hooks, passage names should not begin with a number or include non-alphanumeric characters.
* Twine 1 presentation features such as Stylesheet, Script, Image and Annotation are not supported.