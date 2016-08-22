using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gems.Models.Entity {
  [Table("Kingdom", Schema = "world")]
	public class Kingdom {
		[Key]
		public Guid KingdomId { get; set; }
		public int Id { get; set; }
		[MaxLength(40)]
		public string ReferenceName { get; set; }
		public int BannerArmorBonus { get; set; }
		public int BannerAttackBonus { get; set; }
		public int BannerHealthBonus { get; set; }
		public int BannerSpellPowerBonus { get; set; }
		// public List<Battle> Battles { get; set; }
		public int GoldIncome { get; set; }
		public int GoldMax { get; set; }
		public int LevelRequired { get; set; }
		// public int[] Links { get; set; }
		public int LootTableId { get; set; }
		public Guid? ManaColorId { get; set; }
		public ManaColor ManaColors { get; set; }
		public int NumLinks { get; set; }
		public int PassiveManaBonusPercentage { get; set; }
		// public int[] TroopIds { get; set; }
		public bool Tutorial { get; set; }
		public bool Used { get; set; }
		public int XPos { get; set; }
		public int YPos { get; set; }
		[MaxLength(20)]
		public string BannerDescription { get; set; }
		[MaxLength(20)]
		public string BannerFileBase { get; set; }
		[MaxLength(20)]
		public string BannerManaDescription { get; set; }
		[MaxLength(20)]
		public string BannerName { get; set; }
		[MaxLength(20)]
		public string ByLine { get; set; }
		[MaxLength(20)]
		public string Description { get; set; }
		[MaxLength(20)]
		public string FileBase { get; set; }
		[MaxLength(20)]
		public string Name { get; set; }
		[MaxLength(20)]
		public string PassiveDescription { get; set; }

		public List<KingdomTroop> KingdomTroops { get; set; } = new List<KingdomTroop>();
	}
}