using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gems.Models.Entity {
  [Table("Quest", Schema = "world")]
	public class Quest {
		[Key]
		public Guid QuestId { get; set; }
	}
}