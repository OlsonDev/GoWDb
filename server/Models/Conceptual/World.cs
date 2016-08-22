using System.Collections.Generic;

namespace Gems.Models.Conceptual {
	public class World {
		public List<Entity.AvatarSkin> AvatarSkins { get; set; }
		public List<Entity.Cloud> Clouds { get; set; }
		public List<Entity.ConversationCharacter> ConversationCharacters { get; set; }
		public List<Entity.HeroClass> HeroClasses { get; set; }
		public List<Entity.Kingdom> Kingdoms { get; set; }
		public List<Entity.Quest> Quests { get; set; }
		public List<Entity.Spell> Spells { get; set; }
		public List<Entity.Trait> Traits { get; set; }
		public List<Entity.Troop> Troops { get; set; }
		public List<Entity.Weapon> Weapons { get; set; }
	}
}