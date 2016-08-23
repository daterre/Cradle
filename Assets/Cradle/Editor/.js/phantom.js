/* PhantomJS script that monitors the loading of a story in the windowless browser and waits to receive the output of an injected bridge script */

"use strict";
var system = require('system');
var fs = require('fs');
var page = require('webpage').create();

system.stdout.setEncoding('utf8');

var output = {
	console: []
};

function end(result) {
	var json = JSON.stringify(output,null,4);
	system.stdout.write(json);
	if (filePath)
		fs.write("output.json", json, 'w');
	phantom.exit(result);
}

page.onConsoleMessage = function(msg) {
	output.console.push({type: 'message', value: msg});
};

page.onAlert = function(msg) {
	output.console.push({type: 'alert', value: msg});
};

page.onError = function(msg, trace) {
	var error = {type: 'error', value: msg};
	if (trace)
		error.trace = JSON.stringify(trace,null,4);
	output.console.push(error);
};

page.onCallback = function(data){
	output.result = data;
	end();
};

if (system.args.length < 2) {
	page.onError("Missing URL");
	end(1);
}

var filePath = system.args[1];
var bridgeJsPath = system.args[2];
var bridgeJS;

if (bridgeJsPath) {
	try { bridgeJS = fs.read(bridgeJsPath); }
	catch(ex)
	{
		page.onError(ex);
		end(1);
	}
}

page.open(filePath, function(status) {
	if (status !== 'success') {
		page.onError('URL failed to load');
		end(1);
		return;
	}

	if (bridgeJS) {
		page.evaluate(
			function(bridgeJS) {
				eval(bridgeJS);
			},
			bridgeJS
		);
	}
});

setTimeout(function(){
	page.onError("Timeout while trying to extract the story data.");
	end(1);
},12000);
