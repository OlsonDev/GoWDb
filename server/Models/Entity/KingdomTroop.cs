using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gems.Models.Entity {
  [Table("KingdomToTroop", Schema = "world")]
	public class KingdomTroop {
		public Guid KingdomId { get; set; }
		public Kingdom Kingdom { get; set; }
		public Guid TroopId { get; set; }
		public Troop Troop { get; set; }
	}
}