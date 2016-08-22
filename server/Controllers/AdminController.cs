using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Gems.Models.Db;
using Gems.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Gems.Controllers {
  public class AdminController : BaseController {
		private GameDataDecrypterService _gameDataDecrypterService;
		private ApplicationDbContext _dbContext;

		public AdminController(GameDataDecrypterService decrypterService, ApplicationDbContext dbContext) {
			_gameDataDecrypterService = decrypterService;
			_dbContext = dbContext;
		}

		[HttpPut]
		public IActionResult Upload(ICollection<IFormFile> files) {
			var response = "<no content>";
			foreach (var file in files) {
				if (file.Length == 0) continue;
				var filename = file.FileName.ToLowerInvariant();
				if (filename == "world.json") { response = ProcessWorldJson(file); continue; }
				if (filename == "text.zip") { ProcessTextZip(file); continue; }
			}
			return ApiResponse(() => response);
		}

		private string ProcessWorldJson(IFormFile file) {
			var reader = new StreamReader(file.OpenReadStream());
			var raw = reader.ReadToEnd();
			var json = _gameDataDecrypterService.Decrypt(raw);
			dynamic debugWorld = JObject.Parse(json);
			var world = JsonConvert.DeserializeObject<Models.Conceptual.World>(json);

			_dbContext.Database.BeginTransaction();
			_dbContext.Database.ExecuteSqlCommand("TRUNCATE TABLE world.Kingdom");
			foreach (var kingdom in world.Kingdoms) {
				kingdom.KingdomId = _dbContext.NewGuidComb();
				_dbContext.Add(kingdom);
			}
			_dbContext.SaveChanges();
			_dbContext.Database.CommitTransaction();

			return json;
		}
		private void ProcessTextZip(IFormFile file) {
			// Example .zip file contents
			// text.zip/(en_US|es_ES|fr_FR|it_IT|de_DE)/(Common|Core|Tutorial|World).xml
			var stream = file.OpenReadStream();
			var zip = new System.IO.Compression.ZipArchive(stream, ZipArchiveMode.Read);
			_dbContext.Database.BeginTransaction();
			_dbContext.Database.ExecuteSqlCommand("TRUNCATE TABLE world.Text");
			foreach (var entry in zip.Entries) {
				// entry.FullName     | Name         | Length | LastWriteTime             | CompressedLength
				// -------------------|--------------|--------|---------------------------|-----------------
				// en_US/             |              |      0 | 8/14/2016 01:34:14 -05:00 |      0
				// en_US/Common.xml   | Common.xml   | 161059 | 7/31/2016 21:32:38 -05:00 |  34151
				// en_US/Core.xml     | Core.xml     |   4293 | 7/14/2016 19:39:50 -05:00 |   1579
				// en_US/Tutorial.xml | Tutorial.xml |  13510 | 7/14/2016 19:39:48 -05:00 |   3009
				// en_US/World.xml    | World.xml    | 592042 | 8/14/2016 01:34:14 -05:00 | 126262
				if (entry.Length == 0) continue;
				var locale = entry.FullName.Split('/').First();
				var reader = new StreamReader(entry.Open());
				var xml = Regex.Replace(reader.ReadToEnd(), "&(?!(amp|apos|quot|lt|gt);)", "&amp;");
				ProcessTextXml(locale, entry.Name, xml);
			}
			_dbContext.SaveChanges();
			_dbContext.Database.CommitTransaction();
		}

		private void ProcessTextXml(string locale, string filename, string xml) {
			// Example XML structure
			// <TextLibrary>
			//   <Text tag="[CONNECTING...]">Connecting...</Text>
			//   <Text tag="[HERO]">Hero</Text>
			// </TextLibrary>
			var textLibrary = XElement.Parse(xml);
			foreach (var textElem in textLibrary.Elements("Text")) {
				var text = new Models.Entity.Text();
				text.Locale = locale;
				text.Filename = filename;
				text.Tag = textElem.Attribute("tag").Value;
				text.Value = textElem.Value;
				_dbContext.Add(text);
			}
    }
  }
}