using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Spriter.Services {
	public class GameLocator {

		public string SteamFolder() {
			var steamKey = SoftwareKey64Or32(@"Valve\Steam");
			return steamKey.GetValue("InstallPath").ToString();
		}

		public IEnumerable<string> LibraryFolders() {
			var steamFolder = SteamFolder();
			yield return steamFolder;
			var configFile = $@"{steamFolder}\config\config.vdf";
			var regex = new Regex("BaseInstallFolder[^\"]*\"\\s*\"([^\"]*)\"");
			using (var reader = new StreamReader(File.OpenRead(configFile))) {
				string line;
				while ((line = reader.ReadLine()) != null) {
					var match = regex.Match(line);
					if (match.Success) {
						yield return Regex.Unescape(match.Groups[1].Value);
					}
				}
			}
		}

		private string gameFolder;

		public string GameFolder() {
			if (gameFolder != null) return gameFolder;
			var folder = GameFolderFromSteamLibrary() ?? GameFolderFromUninstallRegistry();
			if (folder == null) {
				throw new DirectoryNotFoundException("Could not find game folder via Steam library nor Windows Uninstall registry.");
			}
			return gameFolder = folder;
		}

		public string GameFolderFromSteamLibrary() {
			var appFolders = LibraryFolders().Select(p => Path.Combine(p, @"SteamApps\common"));
			foreach (var folder in appFolders) {
				try {
					var matches = Directory.GetDirectories(folder, "Gems of War");
					if (matches.Any()) return matches.First();
				} catch (DirectoryNotFoundException) { }
			}
			return null;
		}

		public string GameFolderFromUninstallRegistry() {
			var uninstallKey = SoftwareKey64Or32(@"Microsoft\Windows\CurrentVersion\Uninstall\Steam App 329110");
			if (uninstallKey == null) return null;
			return uninstallKey.GetValue("InstallLocation").ToString();
		}

		private RegistryKey SoftwareKey64Or32(string relativePath) {
			return Registry.LocalMachine.OpenSubKey($@"Software\{relativePath}")
					?? Registry.LocalMachine.OpenSubKey($@"Software\Wow6432Node\{relativePath}")
			;
		}
	}
}