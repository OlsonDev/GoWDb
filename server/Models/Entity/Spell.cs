using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gems.Models.Entity {
  [Table("Spell", Schema = "world")]
	public class Spell {
		[Key]
		public Guid SpellId { get; set; }
	}
}