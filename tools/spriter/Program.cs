using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ImageProcessorCore;
using Spriter.Models;
using Spriter.Services;

namespace Spriter {
  public class Program {
		static private void Main(string[] args) {
			var app = new Program();
			Console.WriteLine("Running...");
			//app.RunExtraction();
			app.RunAtfInfo(args);
			//app.RunRgbaToPng();
		}

		private string gfxDirectory;

		private void RunAtfInfo(string[] args) {
			var path = args.FirstOrDefault();
			var file = AtfFile.FromFile(path);

			Console.WriteLine($"Args: {path}");
			Console.WriteLine($"-------------");
			Console.WriteLine(file);
			Console.WriteLine($"-------------");
		}

		private void RunExtraction() {

			Console.WriteLine("Ensuring extracted directory exists...");
			EnsureExtractedDirectoryExists();

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

		private void EnsureExtractedDirectoryExists() {
			Directory.CreateDirectory("extracted");
		}

		private void ProcessAtlasFiles() {
			Console.WriteLine($"Graphics directory: {gfxDirectory}");
			Console.WriteLine();
			var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			var atlasFiles = Directory.GetFiles(gfxDirectory, "*.xml", SearchOption.AllDirectories);
			foreach (var atlasFile in atlasFiles) {
				// Console.WriteLine($"Reading file: {atlasFile}");
				var atlas = XElement.Load(atlasFile);
				if (atlas.Name.LocalName != "TextureAtlas") continue;
				var imagePath = atlas.Attribute("imagePath").Value;
				var fullImagePath = Path.Combine(Path.GetDirectoryName(atlasFile), imagePath);
				var relativeDirectory = Path.GetDirectoryName(fullImagePath).Replace(gfxDirectory, "").Trim('/', '\\');
				var extractedRelativeDirectory = Path.Combine(assemblyPath, "extracted", relativeDirectory);
				var imageName = Path.GetFileNameWithoutExtension(fullImagePath);
				// Console.WriteLine($"Image path: {imagePath}");
				if (!File.Exists(fullImagePath)) {
					var oldColor = Console.ForegroundColor;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"Could not find file {fullImagePath}");
					Console.ForegroundColor = oldColor;
				}
				// try {
				// 	using (var stream = File.OpenRead(fullImagePath)) {
				// 		var image = new Image(stream);
				// 		foreach (var subTexture in atlas.Elements("SubTexture")) {
				// 			var sprite = Sprite.FromXElement(atlasFile, fullImagePath, subTexture);
				// 			ExtractSprite(image, sprite, extractedRelativeDirectory, imageName);
				// 		}
				// 	}
				// } catch (FileNotFoundException) {
				// 	var oldColor = Console.ForegroundColor;
				// 	Console.ForegroundColor = ConsoleColor.Red;
				// 	Console.WriteLine($"Could not find file {fullImagePath}");
				// 	Console.ForegroundColor = oldColor;
				// }
			}
		}

		private void ExtractSprite(Image sourceImage, Sprite sprite, string extractedRelativeDirectory, string imageName) {
			var outputPath = Path.Combine(extractedRelativeDirectory, $"{imageName}.{sprite.Name}.png");
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