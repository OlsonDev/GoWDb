using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gems.Models.Entity {
  [Table("Weapon", Schema = "world")]
	public class Weapon {
		[Key]
		public Guid WeaponId { get; set; }
	}
}