require(['markup'], function(TwineMarkup){
	"use strict";

	function simplify(tokens) {
		var result = [];
		for (var i = 0; i < tokens.length; i++) {
			var complex = tokens[i];
			var simple = {type: complex.type};

			if (complex.children && complex.children.length && complex.type !== 'string')
				simple.tokens = simplify(complex.children);
			if (complex.text)
				simple.text = complex.text;
			if (complex.innerText)
				simple.innerText = complex.innerText;
			if (complex.value)
				simple.value = complex.value;
			if (complex.passage)
				simple.passage = complex.passage;
			if (complex.name)
				simple.name = complex.name;

			result[i] = simple;
		}
		return result;
	}

	var result = [];

	$('tw-passagedata').each(function(i,p){
		var $p = $(p);
		var passage = {
			Pid: $p.attr('pid'),
			Name: $p.attr('name'),
			Tags: $p.attr('tags'),
			Tokens: simplify(TwineMarkup.lex($p.html()).children)
		};
		result.push(passage);
	});

	if (window.callPhantom)
		window.callPhantom(result);
	else
		console.error('UnityTwine: PhantomJS context not available.');
});
