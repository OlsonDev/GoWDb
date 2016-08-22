using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gems.Models.Entity {
  [Table("ConversationCharacter", Schema = "world")]
	public class ConversationCharacter {
		[Key]
		public Guid ConversationCharacterId { get; set; }
	}
}