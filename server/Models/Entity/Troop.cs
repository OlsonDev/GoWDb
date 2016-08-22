using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gems.Models.Entity {
  [Table("Troop", Schema = "world")]
	public class Troop {
		[Key]
		public Guid TroopId { get; set; }

		public int Id { get; set; }
		public int SpellId { get; set; }
		[MaxLength(40)]
		public string ReferenceName { get; set; }
		public string PrimaryColor { get; set; }
		public int Armor_Base { get; set; }
		public int Attack_Base { get; set; }
		public int Health_Base { get; set; }
		public int SpellPower_Base { get; set; }
		public int PortraitOffsetX { get; set; }
		public int PortraitOffsetY { get; set; }
		public long ReleaseDate { get; set; }

		// public List<int> Ascension_Armor { get; set; }
		// public List<int> Ascension_Attack { get; set; }
		// public List<int> Ascension_Health { get; set; }

		// public List<int> ArmorIncrease { get; set; }
		// public List<int> AttackIncrease { get; set; }
		// public List<int> HealthIncrease { get; set; }
		// public List<int> SpellPowerIncrease { get; set; }
		public Guid? ManaColorId { get; set; }
		public ManaColor ManaColors { get; set; }
		// public List<string> Traits { get; set; }

		[MaxLength(200)]
		public string FileBase { get; set; }
		[MaxLength(40)]
		public string Name { get; set; }
		[MaxLength(40)]
		public string Description { get; set; }
		[MaxLength(20)]
		public string SoundCastSpell { get; set; }
		[MaxLength(20)]
		public string SoundDeath { get; set; }
		[MaxLength(20)]
		public string SoundSelection { get; set; }
		[MaxLength(10)]
		public string TroopRarity { get; set; }
		[MaxLength(20)]
		public string TroopType { get; set; }
		[MaxLength(20)]
		public string TroopType2 { get; set; }

		public List<KingdomTroop> KingdomTroops { get; set; } = new List<KingdomTroop>();
	}
}