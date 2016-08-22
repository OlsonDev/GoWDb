using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gems.Models.Entity {
  [Table("Trait", Schema = "world")]
	public class Trait {
		[Key]
		public Guid TraitId { get; set; }

		[MaxLength(20)]
		public string Code { get; set; }
		[MaxLength(40)]
		public string Activation { get; set; }
		[MaxLength(40)]
		public string Description { get; set; }
		[MaxLength(20)]
		public string Filter { get; set; }
		public decimal Modifier { get; set; }
		[MaxLength(40)]
		public string Name { get; set; }
		[MaxLength(40)]
		public string TraitType { get; set; }
		[MaxLength(20)]
		public string Trigger { get; set; }
	}
}