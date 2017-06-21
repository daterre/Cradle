# Sugar - Cradle story format
A superset of both Sugarcane (Twine 1) and SugarCube (Twine 2) storyformats.

**Note**: Many of the more advanced features of SugarCube are not currently supported. SugarCube doesn't feature a proper lexer and relies heavily on JavaScript and DOM, which makes it hard to transcode to C#. It is therefore recommended to use the Harlowe format, which has a more independent syntax, for new Cradle stories.

- [Supported syntax](#supported-syntax)
	- [Links](#links)
	- [Variables](#variables)
		- [Arrays and Objects](#arrays-and-objects)
	- [Macros](#macros)
	- [Functions](#functions)
	- [Styling](#styling)
	- [General](#general)
- [Custom functions](#custom-functions)

## Supported syntax
Here is a list of what works and what doesn't:

### Links
Links work as expected:

* Simple: `[[passage]]`
* With link text: `[[text|passage]]`
* With variable setters: `[[text|passage][$var = 123]]`
* With expressions: `[[text|either("a", "b")]]`

A syntax extension allows **naming** links for easy reference in Unity scripts:
* `[[continue = Continue down the hall.|hallway]]`

### Variables
* Global variables that start with the `$` sign are supported.
* Naked variables in text will be displayed correctly: `His shirt was $shirtColor`.
* Local variables (starting with `_`) are **not supported yet**.

#### Arrays and Objects
Arrays and objects are not supported out-of-the-box. This is because SugarCube uses JavaScript directly, which is incompatible with C# and the current Regex version of the Sugar transcoder. To compensate for this, an [extension function library](../Editor/js/StoryFormats/Sugar/sugar.extensions.js_) is available. See [here](http://www.motoslave.net/sugarcube/2/docs/special-names.html#special-tags) for an explanation how to add it to your story.

For **arrays**, the following extension functions are available:
* `array($value0, $value1, ...)` - creates an array, equivalent to `[$value0, $value1, ...]` in Twine
* `arrayGet($arr, $index)` - returns the value at the specified index, equivalent to `$arr[$index]` in Twine
* `<<run arraySet($arr, $index, $value)>>` - sets the value at the specified index, equivalent to `<<set $arr[$index] = $value>>` in Twine
* `arrayIndexOf($arr, $value)` - returns the first index of the value or -1 if is not found. Equivalent to `$arr.indexOf($value)` in Twine
* `arrayLength($arr)` - returns the length of the array, equivalent to `$arr.length` in Twine
* `arrayCount($arr, $value)` - return the number of times the value is in the array, equivalent to `$arr.count($value)` in Twine
* `arrayContains($arr, $value)` - checks if a value in the array, equivalent to `$arr.contains($value)` in Twine
* `arrayContainsAll($arr, $value1, $value2)` - checks if all the values are in the array, equivalent to `$arr.containsAll($value1, $value2)` in Twine
* `arrayContainsAny($arr, $value1, $value2)` - checks if any of the values are in the array, equivalent to `$arr.containsAny($value1, $value2)` in Twine
* `arrayAdd($arr, $value)` - adds a value to the end of the array, equivalent to `$arr.push($value)` in Twine
* `arrayInsert($arr, $index, $value)` - inserts a value at a specific position in the array, equivalent to `$arr.splice($index, 0, $value)` in Twine
* `arrayDelete($arr, $value1, $value2, ...)` - removes the specified values, equivalent to  `$arr.delete($value1, $value2, ...)` in Twine
* `arrayDeleteAt($arr, $index1, $index2, ...)` - removes the values at the specified positions, equivalent to  `$arr.deleteAt($index1, $index2, ...)` in Twine

To iterate an array with a `<<for>>` macro:
```
<<for $i = 0; $i < arrayLength($arr); $i++>>
	<<print arrayGet($arr, $i)>>
<</for>>
```

For **objects**, the following extension functions are available:
* `obj("key", $value, "key2", $value2, ...)` - creates a new object, equivalent to `{"key": $value, "key2" : $value2}` in Twine
* `objGet($obj, "key")` - gets a property of the object, equivalent to `$obj["key"]` in Twine
* `<<run objSet($obj, "key", $value)>>` - sets a property of the object, equivalent to `<<set $obj["key"] = $value>>` in Twine
* `objDelete($obj, "key")` - removes a property from the object, equivalent to `delete $obj["key"]` in Twine
* `objContains($obj, "key")` - checks if the object contains a property, equivalent to `$obj.hasOwnProperty("key")` in Twine

### Macros
Macros that work:
* `<<set>>`. `<<run>>`  (both single and multiple variables)
* `<<print>>`, `<<=>>` (**however** it doesn't support dynamic Twine markup, it will just print everything verbatim)
* `<<display>>` (including Sugarcane's [shorthand](#https://twinery.org/wiki/display) syntax)
* `<<goto>>` (aborts the current passage completely)
* `<<if>>` .. `<<else>>` .. `<<endif>>`
* `<<for>>` with `<<continue>>` and `<<break>>` (**however** the shorthand format without init and post expressions is not supported)
* `<<nobr>>`, `<<silently>>`
* `<<back>>`, `<<return>>` (with string parameters only, not with full link syntax)

Macros that **don't work yet**:
* `<<unset>>`, `<<remember>>`, `<<forget>>`
* `<<->>`
* `<<actions>>`, `<<choice>>` 
* `<<click>>`, `<<button>>`
* `<<script>>`
* `<<textinput>>`, `<<radio>`, `<<radiobutton>>`, `<<checkbox>>`, `<<textarea>>`, `<<textbox>>`
* `<<addclass>>`, `<<removeclass>>`, `<<toggleclass>>`
* `<<copy>>`, `<<append>>`, `<<prepend>>`, `<<replace>>`, `<<remove>>`
* `<<repeat>`, `<<timed>>`, `<<stop>>`, `<<next>>`
* `<<widget>>`

### Functions
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

### Styling
Styling is not supported in Sugar. It is outputted as plain text.
None of the following work:
* [Sugarcane formatting](https://twinery.org/wiki/syntax)
* [SugarCube formatting](http://www.motoslave.net/sugarcube/2/docs/markup.html#html-attributes)
* Image tag

### General
* Strings should use `"double quotes"`, not `'single quotes'`.
* Presentation features such as Stylesheet, Script, Image and Annotation are not supported.