using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gems.Models.Entity {
  [Table("AvatarSkin", Schema = "world")]
	public class AvatarSkin {
		[Key]
		public Guid AvatarSkinId { get; set; }
	}
}