using Microsoft.Extensions.Configuration;
using System;

namespace Gems.Services {
	public class GameDataDecrypterService {
		private string decryptFind;
		private string decryptReplace;
		private int[] findReplaceTransform;

		private int[] GetFindReplaceTransform() {
			if (findReplaceTransform != null) return findReplaceTransform;

			var transform = new int[256];
			for (var i = 0; i < transform.Length; i++) {
				transform[i] = i;
			}
			for (var i = 0; i < decryptFind.Length; i++) {
				transform[decryptFind[i]] = decryptReplace[i];
			}

			return findReplaceTransform = transform;
		}

		public GameDataDecrypterService(IConfiguration configuration) {
			decryptFind = configuration["Data:GameDecryptFind"];
			decryptReplace = configuration["Data:GameDecryptReplace"];
		}

		public string Decrypt(string raw) {
			var transform = GetFindReplaceTransform();
			var decrypted = raw.ToCharArray();
			for (var i = 0; i < raw.Length; i++) {
				decrypted[i] = (char)transform[raw[i]];
			}
			return new string(decrypted);
		}
	}
}