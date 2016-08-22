using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gems.Models.Entity {
  [Table("Spell", Schema = "world")]
	public class Spell {
		[Key]
		public Guid SpellId { get; set; }

		// public List<SpellStep> SpellSteps { get; set; }

		public int Id { get; set; }
		public int Cost { get; set; }
		public int OverrideAI { get; set; }
		[MaxLength(20)]
		public string Name { get; set; }
		[MaxLength(20)]
		public string Description { get; set; }
		[MaxLength(20)]
		public string Randomize { get; set; }
		[MaxLength(20)]
		public string Target { get; set; }
	}
}