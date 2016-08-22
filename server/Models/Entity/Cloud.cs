using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gems.Models.Entity {
  [Table("Cloud", Schema = "world")]
	public class Cloud {
		[Key]
		public Guid CloudId { get; set; }
	}
}