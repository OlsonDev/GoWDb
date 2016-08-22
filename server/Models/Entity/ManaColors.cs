using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gems.Models.Entity {
  [Table("ManaColor", Schema = "world")]
	public class ManaColor {
		[Key]
		public Guid ManaColorId { get; set; }
		public bool ColorRed { get; set; }
		public bool ColorYellow { get; set; }
		public bool ColorGreen { get; set; }
		public bool ColorBlue { get; set; }
		public bool ColorPurple { get; set; }
		public bool ColorBrown { get; set; }
		public bool ColorOrange { get; set; }
	}
}