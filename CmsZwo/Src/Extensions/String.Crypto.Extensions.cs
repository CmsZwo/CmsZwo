using System;
using System.Text;
using System.Security.Cryptography;

namespace CmsZwo
{
	public static class StringCryptoExtensions
	{
		#region Password and Salting

		public static string CreateSaltMd5(this string intance)
		{
			using (var hasher = new Rfc2898DeriveBytes(intance, Encoding.Default.GetBytes("waeunalirublaierubl"), 10000))
			{
				return
					Convert.ToBase64String(hasher.GetBytes(25));
			}
		}

		public static string HashPasswordMd5(this string instance, string saltHash)
		{
			if (!instance.HasContent())
				throw new ArgumentException($"[{nameof(instance)}] must have content.");

			if (!saltHash.HasContent())
				throw new ArgumentException($"[{nameof(saltHash)}] must have content.");

			using (var hasher = new Rfc2898DeriveBytes(instance, Encoding.Default.GetBytes(saltHash), 10000))
			{
				return
					Convert.ToBase64String(hasher.GetBytes(25));
			}
		}

		public static bool PasswordMd5Match(this string instance, string saltHash, string passwordHash)
		{
			if (instance == "" && !passwordHash.HasContent()) return true;
			if (!passwordHash.HasContent()) return false;
			if (!saltHash.HasContent()) return false;

			var saltIsHexFormatFromPhp = saltHash.Length < 8;
			if (saltIsHexFormatFromPhp)
			{
				var innerHash = instance.ToMd5Hash(phpCompatible: true);
				var outerHash = (innerHash + saltHash).ToMd5Hash(phpCompatible: true);
				var isMatch = outerHash == passwordHash;
				return isMatch;
			}

			var inputHash = instance.HashPasswordMd5(saltHash);
			var result = inputHash == passwordHash;
			return result;
		}

		#endregion

		#region SHA512

		public static string HashWithSha512(this string instance)
		{
			using (var provider = new SHA512Managed())
			{
				var hash = provider.ComputeHash(Encoding.UTF8.GetBytes(instance));
				var result = BitConverter.ToString(hash);
				result = result.Replace("-", String.Empty).ToLower();
				return result;
			}
		}

		public static bool ValidateWithSha512(this string instance, string content)
		{
			var validationHash = HashWithSha512(content);
			var result = instance.EqualsIgnoreCase(validationHash);
			return result;
		}

		#endregion

		#region SHA1

		public static string HashWithSha1(this string instance)
		{
			var provider = new SHA1Managed();
			var hash = provider.ComputeHash(Encoding.Default.GetBytes(instance));
			provider.Dispose();
			var result = BitConverter.ToString(hash);

			result = result.Replace("-", String.Empty).ToLower();

			return result;
		}

		public static bool ValidateWithSha1(this string instance, string content)
		{
			string validationHash = HashWithSha1(content);
			var result = instance.EqualsIgnoreCase(validationHash);
			return result;
		}

		#endregion

		#region MD5

		public static string ToMd5Hash(this string instance, bool phpCompatible = true)
			=> instance.GetMd5Hash(phpCompatible: phpCompatible);

		public static string GetMd5Hash(this string instance, bool phpCompatible = true)
		{
			if (!instance.HasContent())
				return string.Empty;

			using (var md5 = new MD5CryptoServiceProvider())
			{
				if (!phpCompatible)
				{
					var bytes = Encoding.Default.GetBytes(instance);
					var bytesHashed = md5.ComputeHash(bytes);
					return BitConverter.ToString(bytesHashed);
				}

				var md5Hash = md5.ComputeHash(Encoding.Default.GetBytes(instance));
				var sb = new StringBuilder();

				for (int i = 0; i < md5Hash.Length; i++)
					sb.AppendFormat("{0:x2}", md5Hash[i]);

				return sb.ToString();
			}
		}

		#endregion
	}
}
