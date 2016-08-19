using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using ImageProcessorCore;
using Spriter.Extensions;
using Spriter.Models;
using Spriter.Services;

namespace Spriter
{
    public class Program {
		static private void Main(string[] args) {
			var app = new Program();
			Console.WriteLine("Running...");
			//app.RunExtraction();
			//app.RunAtfInfo(args);
			//app.RunRgbaToPng();
			app.RunBuildSpritesheets();
		}

		private string gfxDirectory;

		private void RunAtfInfo(string[] args) {
			var path = args.FirstOrDefault() ?? @"C:\Program Files (x86)\Steam\steamapps\common\Gems of War\res\Graphics\3\BannersSmall.atf";
			var file = AtfFile.FromFile(path);

			Console.WriteLine($"Args: {path}");
			Console.WriteLine();
			Console.WriteLine(file);

			var pngPath = Path.ChangeExtension(path, "png");
			file.SaveAsPng(pngPath);
		}

		private void RunBuildSpritesheets() {
			var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			var extracted = Path.Combine(assemblyPath, "extracted");
			var baseSpritesheetDirectory = Path.Combine(assemblyPath, "spritesheets");
			var sheets = new List<Spritesheet> {
				new Spritesheet("banners"     , @"^Banners\\K[0-9]{2}\.png$", filename => filename.ToLower()),
				new Spritesheet("classes"     , @"^Classes\.(?!blade|comingsoon)[a-z]+\.png$", filename => filename.Replace("Classes.", "")),
				new Spritesheet("class-perks" , @"^ClassMenu2\.aug_.+\.png$", filename => filename.Replace("ClassMenu2.aug_", "")),
				new Spritesheet("flags"       , @"^Options\.(de_DE|en_US|es_ES|fr_FR|it_IT|jp_JP|kr_KR|zh_CH|)\.png$", filename => filename.Replace("Options.", "").Replace('_', '-')),
				new Spritesheet("kingdoms"    , @"^(MapLocations\.K[0-9]{2}|TroopCollection\.tab_(k32|k33|k34|hero))\.png$", filename => filename.Replace("MapLocations.", "").Replace("TroopCollection.tab_", "").ToLower()),
				new Spritesheet("mana-colors" , @"^TroopCardAll\.(All|(Red|Orange|Yellow|Blue|Green|Purple|Brown){1,3})\.png$", filename => filename.Replace("TroopCardAll.", "").Replace("Orange", "Brown").PascalToSlug()) { MaxWidth = 1024, MaxHeight = 1024 },
				new Spritesheet("misc"        , @"^((CraftingMenu\.(can_ascend|can_upgrade|can_upgrade_ascend))|(KingdomMenu\.(Attack|Heart|Magic|glory|gold|goldperday|goldperkingdom|rarity_stars|star[01]|star_off))|(MapLocations\.(kstars[0-5]|levelshield_[0-3]|nameplate.*))|(PuzzleIntro\.ButtonArrow.*)|(TroopCardAll\.(Armor|Armor_zero|TurnIndicator|card_level|disenchant_soul|stars_[0-5]|trait_flag_small_[1-3]))|(TroopCardTraits\.(trait_flag_[1-3]))|(TroopCollection\.(icon_kingdoms)))\.png$", filename => {
					filename = Regex.Replace(filename, @"^(CraftingMenu|KingdomMenu|MapLocations|PuzzleIntro|TroopCardAll|TroopCardTraits|TroopCollection)\.", "");
					switch (filename) {
						case "ButtonArrow.png":     return "btn-left.png";
						case "ButtonArrow2.png":    return "btn-right.png";
						case "cardlevel.png":       return "troop-level.png";
						case "disenchant_soul.png": return "soul.png";
						case "goldperday.png":      return "gold-chest.png";
						case "goldperkingdom.png":  return "gold-coin-stacks.png";
						case "icon_kingdoms.png":   return "crown.png";
						case "TurnIndicator.png":   return "turn-indicator.png";
					}
					filename = Regex.Replace(filename, @"^star([01])", @"star-$1");
					filename = Regex.Replace(filename, @"^kstars", "stars-blk-");
					filename = Regex.Replace(filename, @"^levelshield", "level-shield");
					filename = Regex.Replace(filename, @"^trait_flag_([1-3])", "trait-v-$1");
					filename = Regex.Replace(filename, @"^trait_flag_small", "trait-h");
					return filename.Replace('_', '-').ToLower();
				}),
				new Spritesheet("traits"     , @"^Traits\.[a-z]+\.png$", filename => filename.Replace("Traits.", "")),
				new Spritesheet("traitstones", @"^LargeRunes.Rune[0-9]{2}\.png$", filename => filename.Replace("LargeRunes.Rune", "rune")),
				new Spritesheet("troop-card" , @"^TroopCard(Large|Spell).rare[0-9]\.png$", filename => filename.Replace("TroopCardLarge.rare", "frame-rarity-").Replace("TroopCardSpell.rare", "spell-rarity-")) { MaxWidth = 4096, MaxHeight = 4096 }
			};


			foreach (var sheet in sheets) {
				var files = Directory.GetFiles(extracted, "*.png", SearchOption.AllDirectories);
				var filtered = files.Where(path => Regex.IsMatch(path.Replace($@"{extracted}\", ""), sheet.Pattern)).ToList();
				var spritesheetDirectory = Path.Combine(baseSpritesheetDirectory, sheet.Name);
				Directory.CreateDirectory(spritesheetDirectory);
				foreach (var file in filtered) {
					var renamed = sheet.Renamer(Path.GetFileName(file));
					File.Copy(file, Path.Combine(spritesheetDirectory, renamed), true);
				}

				var cmd = new ProcessStartInfo();
				cmd.FileName = @"C:\Program Files\CodeAndWeb\TexturePacker\bin\TexturePacker.exe";
				cmd.Arguments = sheet.BuildTexturePackerArguments();
				cmd.WorkingDirectory = baseSpritesheetDirectory;
				var process = Process.Start(cmd);
				process.WaitForExit();

				var atlas = XElement.Load(Path.Combine(baseSpritesheetDirectory, $"{sheet.Name}.xml"));
				var width = int.Parse(atlas.Attribute("width").Value);
				var height = int.Parse(atlas.Attribute("height").Value);
				var img = new Image(width, height);
				var pixels = img.Pixels;
				foreach (var sprite in atlas.Elements("sprite")) {
					var spriteFile = Path.Combine(spritesheetDirectory, $"{sprite.Attribute("n").Value}.png");
					var spriteImage = new Image(File.OpenRead(spriteFile));
					var spritePixels = spriteImage.Pixels;
					var x = int.Parse(sprite.Attribute("x").Value);
					var y = int.Parse(sprite.Attribute("y").Value);
					var w = int.Parse(sprite.Attribute("w").Value);
					var h = int.Parse(sprite.Attribute("h").Value);
					var oX = int.Parse(sprite.Attribute("oX")?.Value ?? "0");
					var oY = int.Parse(sprite.Attribute("oY")?.Value ?? "0");
					var oW = int.Parse(sprite.Attribute("oW")?.Value ?? sprite.Attribute("w").Value);
					for (var i = 0; i < h; i++) {
						for (var j = 0; j < w; j++) {
							pixels[(y + i) * width + x + j] = spritePixels[(i + oY) * oW + j + oX];
						}
					}
				}
				img.Save(File.OpenWrite(Path.Combine(baseSpritesheetDirectory, $"{sheet.Name}.png")));
			}
		}

		private void RunExtraction() {
			var locator = new GameLocator();
			gfxDirectory = Path.GetFullPath(Path.Combine(locator.GameFolder(), "res/Graphics/3"));
			Console.WriteLine("Processing atlas files...");
			ProcessAtlasFiles();
		}

		private void RunRgbaToPng() {
			var file = File.OpenRead(@"C:\Derp\output.rgba");
			var reader = new BinaryReader(file);
			var image = new Image(512, 1024);
			var pixels = image.Pixels;
			var i = 0;
			for (var x = 0; x < 512; x++) {
				for (var y = 0; y < 1024; y++) {
					pixels[i] = new Color(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
					i++;
				}
			}
			image.Save(File.OpenWrite(@"C:\Derp\output.png"));
		}

		private void ProcessAtlasFiles() {
			Console.WriteLine($"Graphics directory: {gfxDirectory}");
			Console.WriteLine();
			var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			var atlasFiles = Directory.GetFiles(gfxDirectory, "*.xml", SearchOption.AllDirectories);
			Parallel.ForEach(atlasFiles, atlasFile => {
				var atlas = XElement.Load(atlasFile);
				if (atlas.Name.LocalName != "TextureAtlas") return;
				var imagePath = atlas.Attribute("imagePath").Value;
				var fullImagePath = Path.Combine(Path.GetDirectoryName(atlasFile), imagePath);
				var relativeDirectory = Path.GetDirectoryName(fullImagePath).Replace(gfxDirectory, "").Trim('/', '\\');
				var extractedRelativeDirectory = Path.Combine(assemblyPath, "extracted", relativeDirectory);
				var imageName = Path.GetFileNameWithoutExtension(fullImagePath);
				var shortName = Path.GetFileName(atlasFile);
				Console.WriteLine($"Atlas: {shortName} / Image: {imagePath}");
				if (!File.Exists(fullImagePath)) {
					var oldColor = Console.ForegroundColor;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"Could not find file {fullImagePath}");
					Console.ForegroundColor = oldColor;
				}
				try {
					using (var stream = File.OpenRead(fullImagePath)) {
						var image = new Image(stream);
						foreach (var subTexture in atlas.Elements("SubTexture")) {
							var sprite = Sprite.FromXElement(atlasFile, fullImagePath, subTexture);
							ExtractSprite(image, sprite, extractedRelativeDirectory, imageName);
						}
					}
				} catch (FileNotFoundException) {
					var oldColor = Console.ForegroundColor;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"Could not find file {fullImagePath}");
					Console.ForegroundColor = oldColor;
				}
			});
		}

		private void ExtractSprite(Image sourceImage, Sprite sprite, string extractedRelativeDirectory, string imageName) {
			var spriteName = sprite.Name.Replace('/', '.').Replace('\\', '.');
			var outputFilename = imageName == spriteName ? $"{imageName}.png" : $"{imageName}.{spriteName}.png";
			var outputPath = Path.Combine(extractedRelativeDirectory, outputFilename);
			if (File.Exists(outputPath)) return;
			Console.WriteLine($"Extracting to: {outputPath}");
			Directory.CreateDirectory(extractedRelativeDirectory);
			using (var output = File.OpenWrite(outputPath)) {
				var source = new Rectangle(sprite.X, sprite.Y, sprite.Width, sprite.Height);
				var chained = sourceImage.Crop(sprite.Width, sprite.Height, source);
				if (sprite.Rotated) {
					chained = chained.Rotate(-90);
				}
				chained.Save(output);
			}
		}
	}
}