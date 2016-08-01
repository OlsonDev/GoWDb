let fs = require('fs-extra');
let copy = (path) => fs.copySync(`node_modules/${path}`, `public/nm/${path}`);

// <script> tags
copy('core-js/client/shim.min.js');
copy('zone.js/dist/zone.js');
copy('reflect-metadata/Reflect.js');
copy('systemjs/dist/system.src.js');

// System.js require()'d
copy('@angular');
copy('angular2-in-memory-web-api');
copy('rxjs');