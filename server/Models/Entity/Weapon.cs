using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gems.Models.Entity {
  [Table("Weapon", Schema = "world")]
	public class Weapon {
		[Key]
		public Guid WeaponId { get; set; }

		// public ManaColors<bool> ManaColors { get; set; }

		public int Id { get; set; }
		public int SpellId { get; set; }
		public int MasteryRequirement { get; set; }
		public int OverrideAI { get; set; }
		[MaxLength(20)]
		public string Name { get; set; }
		[MaxLength(20)]
		public string ReferenceName { get; set; }
		[MaxLength(20)]
		public string WeaponRarity { get; set; }
		[MaxLength(20)]
		public string FileBase { get; set; }
	}
}