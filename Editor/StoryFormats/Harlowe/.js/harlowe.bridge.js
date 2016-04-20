(function() {
	'use strict';

	var phantomExit = function(result) {
		if (window.callPhantom)
			window.callPhantom(result);
		else
			console.error('UnityTwine: PhantomJS context not available.');
	};

	var $storydata = $('tw-storydata');
	if (!$storydata.length)
	{
		console.error("The file is not a valid Twine 2 story.");
		phantomExit();
		return;
	}
	if ($storydata.attr("format") != "Harlowe")
	{
		console.error("The story was not created with the Harlowe story format.");
		phantomExit();
		return;
	}

	window.unityTwineHarloweBridge = function($, require) {

		require(['markup', 'utils'], function(TwineMarkup, utils){
			"use strict";

			function simplify(tokens, collapsed) {
				var result = [];
				for (var i = 0; i < tokens.length; i++) {
					var complex = tokens[i];
					
					var simple = {type: complex.type};

					// special cases
					switch(complex.type) {
						case "twineLink": {
							// Desugar links
							complex = TwineMarkup.lex("(link-goto:"
								+ utils.toJSLiteral(complex.innerText) + ","
								+ utils.toJSLiteral(complex.passage) + ")"
							);
							continue;
						}
					}

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

			phantomExit(result);
		});
	};

	var req = $('<script/>')
		.attr('role', 'script')
		.attr('id', 'unitytwine-bridge')
		.attr('type', 'text/twine-javascript')
		.html("window.unityTwineHarloweBridge($, require);")
		.insertBefore('#twine-user-script');
}());