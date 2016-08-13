import { Component } from '@angular/core';

@Component({
	moduleId: module.id
	, selector: 'gems-admin'
	, templateUrl: '/views/admin.html'
	, styleUrls: [ '../css/admin.component.css' ]
})
export class AdminComponent {
	links: Link[] = [
		new Link('/kingdoms', 'Kingdoms', 'home', ''),
		new Link('/troops'  , 'Troops'  , 'pets', ''),
		new Link('/weapons' , 'Weapons' , '', ''),
		new Link('/classes' , 'Classes' , 'group', ''),
		new Link('/quests'  , 'Quests'  , 'question answer', ''),
		new Link('/admin'   , 'Admin'   , 'security', ''),
	];
}

export class Link {
	constructor(public href = '', public name = '', public icon = '', public description = '') {
	}
}