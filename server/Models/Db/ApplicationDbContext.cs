using Gems.Models.Attributes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;
using Gems.Models.Entity;

namespace Gems.Models.Db {
	public class ApplicationDbContext : DbContext {
		// public DbSet<AvatarSkin> AvatarSkins { get; set; }
		// public DbSet<Cloud> Clouds { get; set; }
		// public DbSet<ConversationCharacter> ConversationCharacters { get; set; }
		// public DbSet<HeroClass> HeroClasses { get; set; }
		public DbSet<Kingdom> Kingdoms { get; set; }
		// public DbSet<Quest> Quests { get; set; }
		// public DbSet<Spell> Spells { get; set; }
		// public DbSet<Trait> Traits { get; set; }
		public DbSet<Troop> Troops { get; set; }
		// public DbSet<Weapon> Weapons { get; set; }

		public DbSet<Text> Texts { get; set; }
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			BuildModelSchemaNames(modelBuilder);

			modelBuilder.Entity<Kingdom>()
				.HasIndex(e => new { e.Id })
				.IsUnique()
			;

			base.OnModelCreating(modelBuilder);
		}

		private static readonly long BaseDateTicks = new DateTime(1900, 1, 1).Ticks;

		public Guid NewGuidComb() {
			var guidBytes = Guid.NewGuid().ToByteArray();
			var now = DateTime.UtcNow;
			var days = TimeSpan.FromTicks(now.Ticks - BaseDateTicks).Days;
			var dayBytes = BitConverter.GetBytes(days);
			// SQL Server is accurate to 1/300th of a millisecond
			var msecBytes = BitConverter.GetBytes((long)(1000 * now.TimeOfDay.TotalMilliseconds / 300));
			// Match SQL Server's byte order
			Array.Reverse(dayBytes);
			Array.Reverse(msecBytes);
			// (Comb)ine GUID with timestamp
			Array.Copy(dayBytes, dayBytes.Length - 2, guidBytes, guidBytes.Length - 6, 2);
			Array.Copy(msecBytes, msecBytes.Length - 4, guidBytes, guidBytes.Length - 4, 4);
			return new Guid(guidBytes);
		}

		private static void BuildModelSchemaNames(ModelBuilder modelBuilder) {
			var asm = typeof(ApplicationDbContext).GetTypeInfo().Assembly;
			var typesWithSchemas = asm
				.GetTypes()
				.Select(t => new { Type = t, Attr = t.GetTypeInfo().GetCustomAttribute<SchemaNameAttribute>() })
				.Where(o => o.Attr != null)
			;
			foreach (var typeAndAttr in typesWithSchemas) {
				modelBuilder.Entity(typeAndAttr.Type).ToTable(typeAndAttr.Type.Name, typeAndAttr.Attr.SchemaName);
			}
		}
	}
}