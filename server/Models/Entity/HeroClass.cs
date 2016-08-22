using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gems.Models.Entity {
  [Table("HeroClass", Schema = "world")]
	public class HeroClass {
		[Key]
		public Guid HeroClassId { get; set; }
	}
}