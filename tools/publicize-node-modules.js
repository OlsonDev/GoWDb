let fs = require('fs-extra');
let copyNM = (path) => fs.copySync(`node_modules/${path}`, `public/nm/${path}`);
let copyBC = (path) => fs.copySync(`bower_components/${path}`, `public/bower_components/${path}`);

// TODO: Be more selective...
copyBC('');

// <script> tags
// copy('core-js/client/shim.min.js');
// copy('zone.js/dist/zone.js');
// copy('reflect-metadata/Reflect.js');
// copy('systemjs/dist/system.src.js');

// System.js require()'d
// copy('@angular');
// copy('angular2-in-memory-web-api');
// copy('rxjs');