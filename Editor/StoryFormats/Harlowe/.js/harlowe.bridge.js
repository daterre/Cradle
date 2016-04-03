require(['markup', 'utils'], function(TwineMarkup, utils){
	"use strict";

	function simplify(tokens, collapsed) {
		var result = [];
		for (var i = 0; i < tokens.length; i++) {
			var complex = tokens[i];
			
			// special cases
			switch(complex.type) {
				case "twineLink": {
					// Desugar links
					complex = TwineMarkup.lex("(link-goto:"
						+ utils.toJSLiteral(complex.innerText) + ","
						+ utils.toJSLiteral(complex.passage) + ")"
					);
				}

				case "collapsed": {
					// Continue by skipping children
					result = result.concat(simplify(complex.children, true));
					continue;
				}

				case "br": {
					// Skip line breaks when collapsed
					if (collapsed)
						continue;
				}
			}

			var simple = {type: complex.type};

			if (complex.children && complex.children.length && complex.type !== 'string' && complex.type !== 'verbatim')
				simple.tokens = simplify(complex.children, collapsed);
			if (complex.text)
				simple.text = $('<div/>').html(complex.text).text();
			if (complex.innerText)
				simple.innerText = complex.innerText;
			if (complex.value)
				simple.value = complex.value;
			if (complex.passage)
				simple.passage = complex.passage;
			if (complex.name)
				simple.name = complex.type === "macro" ?
					utils.insensitiveName(complex.name) :
					complex.name;

			result.push(simple);
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
