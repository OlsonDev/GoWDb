import { provideRouter, RouterConfig } from '@angular/router';

import { HomeComponent } from './home.component';

import { KingdomsComponent } from './kingdoms.component';
import { KingdomComponent } from './kingdom.component';

import { TroopsComponent } from './troops.component';
import { TroopComponent } from './troop.component';

import { WeaponsComponent } from './weapons.component';
import { WeaponComponent } from './weapon.component';

import { HeroClassesComponent } from './hero-classes.component';
import { HeroClassComponent } from './hero-class.component';

import { QuestsComponent } from './quests.component';
import { QuestComponent } from './quest.component';

import { AdminComponent } from './admin.component';

import { PageNotFoundComponent } from './page-not-found.component';


const routes: RouterConfig = [
	{ path: '', component: HomeComponent }

  , { path: 'kingdoms', component: KingdomsComponent }
  , { path: 'kingdom/:id', component: KingdomComponent }

	, { path: 'troops', component: TroopsComponent }
	, { path: 'troop/:id', component: TroopComponent }

	, { path: 'weapons', component: WeaponsComponent }
	, { path: 'weapon/:id', component: WeaponComponent }

	, { path: 'classes', component: HeroClassesComponent }
	, { path: 'class/:id', component: HeroClassComponent }

	, { path: 'quests', component: QuestsComponent }
	, { path: 'quest/:id', component: QuestComponent }

	, { path: 'admin', component: AdminComponent }

  , { path: '**', component: PageNotFoundComponent }
];

export const appRouterProviders = [
  provideRouter(routes)
];