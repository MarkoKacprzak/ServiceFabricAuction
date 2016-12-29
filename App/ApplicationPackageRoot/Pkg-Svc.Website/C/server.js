//require('httpsys').slipstream();
var http = require('http');
var path = require('path');
var bunyan = require('bunyan');
var seq = require('bunyan-seq');

var log = bunyan.createLogger({
    name: 'myapp',
    streams: [
        {
            stream: process.stdout,
            level: 'warn',
        },
        seq.createStream({
            serverUrl: 'http://localhost:5341', //Install https://getseq.net/Download
            level: 'info'
        })
    ]
});

log.info('Start HTTP at http://localhost:8080/');

var fs = require('fs');
var rootFolder = __dirname;
var page404 = path.join(rootFolder, '404.html');

var extensions = {
    ".html": "text/html",
    ".css": "text/css",
    ".js": "application/javascript",
    ".png": "image/png",
    ".gif": "image/gif",
    ".jpg": "image/jpeg",
    ".ttf": "application/font-ttf",
    ".woff": "application/font-woff",
};

//helper function handles file verification
function getFile(pathname, res, mimeType) {
    fs.readFile(pathname, function (err, contents) {
        if (!err) {
            res.writeHead(200, {
                "Content-type": mimeType,
                "Content-Length": contents.length
            });
            res.end(contents);
        } else {
            log.info(err);
            console.dir(err);
            fs.readFile(page404, function (err, contents) {
                if (!err) {
                    res.writeHead(404, { 'Content-Type': 'text/html' });
                    res.end(contents);
                } else {
                    log.info(err);
                    console.dir(err);                    
                };
            });
        };
    });
};

function requestHandler(req, res) {
    var pathname = path.join(rootFolder, (req.url === '/' ? 'index.html' : req.url));
    var ext = path.extname(pathname);

    log.info('RequestHandler url:' + req.url);

    if (!extensions[ext]) {
        log.info('Extension not found: ' + ext);
        console.dir("Extension not found: " + ext);
        res.writeHead(404, { 'Content-Type': 'text/html' });
        res.end("&lt;html&gt;&lt;head&gt;&lt;/head&gt;&lt;body&gt;The requested file type is not supported&lt;/body&gt;&lt;/html&gt;");
    } else {
        getFile(pathname, res, extensions[ext]);
    }
};

http.createServer(requestHandler).listen(8080);
//http.createServer(requestHandler).listen('http://*:8080/'); //8080
