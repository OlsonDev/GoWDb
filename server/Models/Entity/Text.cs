using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gems.Models.Entity {
  [Table("Text", Schema = "world")]
	public class Text {
		[Key]
		public Guid TextId { get; set; }
		[MaxLength(20)]
		public string Locale { get; set; }
		[MaxLength(20)]
		public string Filename { get; set; }
		[MaxLength(100)]
		public string Tag { get; set; }
		[MaxLength(500)]
		public string Value { get; set; }
	}
}