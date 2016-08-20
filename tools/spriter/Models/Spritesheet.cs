using System;

namespace Spriter.Models {
	public class Spritesheet {
		public string Name { get; private set; }
		public string Pattern { get; private set; }
		public Func<string, string> Renamer { get; private set; }
		public int MaxWidth { get; set; }
		public int MaxHeight { get; set; }
		public bool ForceSquare { get; set; }
		public Spritesheet (string name, string pattern, Func<string, string> renamer = null) {
			Name = name;
			Pattern = pattern;
			Renamer = renamer ?? ((filename) => filename);
		}

		public string BuildTexturePackerArguments() {
			var args = "";
			args += $"--sheet {Name}.png ";
			args += $"--data {Name}.xml --format xml ";
			args += $"--trim-sprite-names --disable-rotation ";
			if (MaxWidth > 0)  args += $"--max-width {MaxWidth} ";
			if (MaxHeight > 0) args += $"--max-height {MaxHeight} ";
			if (ForceSquare)   args += "--force-squared ";
			args += $"--extrude 0 --shape-padding 2 --trim-mode Trim ";
			args += Name;
			return args;
		}
  }
}