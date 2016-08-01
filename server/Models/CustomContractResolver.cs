using Gems.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace Gems.Models {
	public class CustomContractResolver : DefaultContractResolver {
		public static readonly CustomContractResolver Instance = new CustomContractResolver();

		private CustomContractResolver() {
		}

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
			var property = base.CreateProperty(member, memberSerialization);
			property.PropertyName = property.PropertyName.FirstCharacterToLower();
			return property;
		}
	}
}