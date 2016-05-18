/*
Paste this script into your story's JavaScript
section to add Cradle support for arrays and objects.
*/

function args(args)
{
	var arr = Array.prototype.slice.call(args);
	arr.splice(0, 1);
	return arr;
}

// ................................
// Arrays

window.array = function() {
	return Array.prototype.slice.call(arguments); 
}

window.arrayGet = function(arr, index) {
	return arr[index];
}

window.arraySet = function(arr, index, value) {
	arr[index] = value;
}

window.arrayLength = function(arr) {
	return arr.length;
}

window.arrayIndexOf = function(arr, value) {
	return arr.indexOf(value);
}

window.arrayAdd = function(arr, value) {
	arr.push(value);
}

window.arrayInsert = function(arr, index, value) {
	arr.splice(index, 0, value);
}

window.arrayDelete = function(arr) {
	return Array.prototype.delete.apply(arr, args(arguments));
}

window.arrayDeleteAt = function(arr) {
	return Array.prototype.deleteAt.apply(arr, args(arguments));
}

window.arrayContains = function(arr) {
	return Array.prototype.contains.apply(arr, args(arguments));
}

window.arrayContainsAll = function(arr) {
	return Array.prototype.containsAll.apply(arr, args(arguments));
}

window.arrayContainsAny = function(arr) {
	return Array.prototype.containsAny.apply(arr, args(arguments));
}

window.arrayCount = function(arr) {
	return Array.prototype.count.apply(arr, args(arguments));
}

// ................................
// Objects

window.obj = function()
{
	var o = {};
	for (var i = 0; i < arguments.length; i+=2)
		o[arguments[i]] = arguments[i+1];

	return o;
}

window.objGet = function(obj, key)
{
	return obj[key];
}

window.objSet = function(obj, key, value)
{
	obj[key] = value;
}

window.objDelete = function(obj, key)
{
	delete obj[key];
}

window.objContains = function (obj, key)
{
	return obj.hasOwnProperty(key);
}

