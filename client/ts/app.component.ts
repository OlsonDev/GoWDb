import { ROUTER_DIRECTIVES } from '@angular/router';
import { Component } from '@angular/core';

@Component({
  moduleId: module.id
  , selector: 'gems-app'
	, templateUrl: '/views/app.html'
	, styleUrls: [ '../css/app.component.css' ]
  , directives: [ ROUTER_DIRECTIVES ]
})
export class AppComponent {

}