using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gems.Models.Entity {
  [Table("Troop", Schema = "world")]
	public class Troop {
		[Key]
		public Guid TroopId { get; set; }
	}
}