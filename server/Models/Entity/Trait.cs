using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gems.Models.Entity {
  [Table("Trait", Schema = "world")]
	public class Trait {
		[Key]
		public Guid TraitId { get; set; }
	}
}