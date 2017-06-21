(function() {
	'use strict';

	var phantomExit = function(result) {
		if (window.callPhantom)
			window.callPhantom(result);
		else
			console.error('UnityTwine: PhantomJS context not available.');
	};

	var storeArea = document.querySelector("#storeArea");
	if (storeArea === null)
		storeArea = document.querySelector("#store-area");
	if (storeArea === null)
	{
		console.error("The file is not a valid SugarCube/Sugarcane story.");
		phantomExit();
		return;
	}

	var htmlDecoder = document.createElement('textarea');
	function htmlDecode(text)
	{
		htmlDecoder.innerHTML = text;
		return htmlDecoder.value
			.replace(/\\n/g, "\n")
			.replace(/\\t/g, "\t");
	}

	var result = {
		passages:[]
	};

	var storyData = storeArea.querySelector('tw-storydata');
	if (storyData !== null)
	{
		// Twine 2
		// ...................

		if ($(storyData).attr("format") != "SugarCube")
		{
			console.error("The Twine 2 story was not created with the SugarCube story format.");
			window.callPhantom();
			return;
		}

		// Start passage
		result.startPid = $(storyData).attr('startnode');

		// Passages data
		$(storyData).find('tw-passagedata').each(function(i,p){
			var $p = $(p);
			result.passages.push( {
				Pid: $p.attr('pid'),
				Name: htmlDecode($p.attr('name')),
				Tags: htmlDecode($p.attr('tags')),
				Body: htmlDecode($p.html())
			});
		});
	}
	else
	{
		// Twine 1
		// ...................

		// No start passage in Twine 1
		result.startPid = null;

		// Passages data
		var passages = storeArea.querySelectorAll('div[tiddler]');
		for (var i = 0; i < passages.length; i++) {
			var p = passages[i];
			result.passages.push({
				Pid: i.toString(),
				Name: htmlDecode(p.getAttribute('tiddler')),
				Tags: htmlDecode(p.getAttribute('tags')),
				Body: htmlDecode(p.innerHTML)
			});
		}
	}

	phantomExit(result);
}());