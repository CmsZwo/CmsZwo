using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections.Generic;

namespace CmsZwo
{
	public static class StringExtensions
	{
		#region null or Empty

		public static bool HasContent(this string instance)
			=> !string.IsNullOrWhiteSpace(instance);

		public static bool HasContent(this char instance)
			=> instance.ToString().HasContent();

		#endregion

		#region Base64

		public static byte[] Base64ToByteArray(this string instance)
			=> Convert.FromBase64String(FixBase64ForImage(instance));

		private static string FixBase64ForImage(string str)
			=>
			str
				.Replace("\r\n", String.Empty)
				.Replace(" ", String.Empty);

		#endregion

		#region Formats checking

		public static bool IsAlphaNumeric(this string instance)
			=> Regex.IsMatch(instance, "^[A-Za-z0-9ÄäÖöÜüß]+$", RegexOptions.IgnoreCase);

		public static bool IsInt(this string instance)
		{
			if (!instance.HasContent())
				return false;

			return
				Int32.TryParse(instance, out int trash);
		}

		public static bool IsValidEMail(this string instance)
		{
			if (!instance.HasContent())
				return false;

			return
				Regex.IsMatch(instance, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
		}

		private static string _NumberGroupSeparator;
		private static string NumberGroupSeparator
		{
			get
			{
				if (_NumberGroupSeparator == null)
					_NumberGroupSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;

				return _NumberGroupSeparator;
			}
		}

		private static string _NumberDecimalSeparator;
		private static string NumberDecimalSeparator
		{
			get
			{
				if (_NumberDecimalSeparator == null)
					_NumberDecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

				return _NumberDecimalSeparator;
			}
		}

		public static bool IsNumeric(this string instance)
		{
			if (!instance.HasContent()) return false;

			var hasDecimal = false;
			var foundDigit = false;

			var indexNumberGroupSeparator = instance.IndexOf(NumberGroupSeparator);
			var indexNumberDecimalSeparator = instance.IndexOf(NumberDecimalSeparator);
			if (indexNumberGroupSeparator >= 0 && indexNumberDecimalSeparator >= 0)
				if (indexNumberGroupSeparator > indexNumberDecimalSeparator)
					return false;

			instance = instance?.Replace(NumberGroupSeparator, "");
			if (!instance.HasContent()) return false;

			if (instance == "-") return false;
			if (instance[0] == '-') instance = instance.Substring(1);

			for (var i = 0; i < instance.Length; i++)
			{
				// Check for decimal
				if (instance[i] == NumberDecimalSeparator[0])
				{
					if (hasDecimal) // 2nd decimal
						return false;
					else // 1st decimal
					{
						if (!foundDigit)
							return false;

						// inform loop decimal found and continue 
						hasDecimal = true;
						continue;
					}
				}
				// check if number
				if (!char.IsNumber(instance[i])) return false;
				else foundDigit = true;
			}

			return true;
		}

		#endregion

		#region Formats

		public static bool IsIban(this string instance)
		{
			if (!instance.HasContent())
				return false;

			instance = instance.Replace(" ", "");

			if (instance.Length < 4 || instance[0] == ' ' || instance[1] == ' ' || instance[2] == ' ' || instance[3] == ' ')
				return false;

			var checksum = 0;
			var instanceLength = instance.Length;
			for (int charIndex = 0; charIndex < instanceLength; charIndex++)
			{
				if (instance[charIndex] == ' ')
					continue;

				int value;
				var c = instance[(charIndex + 4) % instanceLength];

				if ((c >= '0') && (c <= '9'))
				{
					value = c - '0';
				}
				else if ((c >= 'A') && (c <= 'Z'))
				{
					value = c - 'A';
					checksum = (checksum * 10 + (value / 10 + 1)) % 97;
					value %= 10;
				}
				else if ((c >= 'a') && (c <= 'z'))
				{
					value = c - 'a';
					checksum = (checksum * 10 + (value / 10 + 1)) % 97;
					value %= 10;
				}
				else
					return false;

				checksum = (checksum * 10 + value) % 97;
			}

			return checksum == 1;
		}

		public static string LengthSafe(this string instance, int maxLength, string tail = "...")
		{
			if (
				!instance.HasContent()
				|| maxLength < 1
				)
				return "";

			if (instance.Length <= maxLength)
				return instance;

			var tailLength =
				tail.HasContent()
				? tail.Length
				: 0;

			var sublength = maxLength - tailLength;
			if (sublength <= 0)
				return instance.Substring(0, maxLength);

			var result = instance.Substring(0, sublength) + tail;
			return result;
		}

		public static string FillLeft(this string instance, string placeholder, int totalLength)
		{
			var fillLength = totalLength;
			if (instance.HasContent())
				fillLength = Math.Max(0, totalLength - instance.Length);

			var fill = "";
			for (int i = 0; i < fillLength; i++)
				fill += placeholder;

			var result = fill + instance;
			return result;
		}

		#endregion

		#region Filtering

		public static string FilterAlpha(this string instance)
		{
			if (!instance.HasContent())
				return instance;

			var result = "";

			for (int i = 0; i < instance.Length; i++)
			{
				var pass = char.IsLetter(instance[i]);
				if (pass) result += instance[i];
			}

			return result;
		}

		public static string FilterAlphaNumeric(this string instance, IEnumerable<char> additional = null, string replace = null)
		{
			if (!instance.HasContent())
				return instance;

			var result = "";

			for (int i = 0; i < instance.Length; i++)
			{
				var pass = char.IsLetterOrDigit(instance[i]) || (additional != null && additional.Contains(instance[i]));
				if (pass) result += instance[i];
				else result += replace;
			}

			return result;
		}

		public static string FilterUrlAllowed(this string instance)
		{
			if (!instance.HasContent())
				return instance;

			var result = "";
			for (int i = 0; i < instance.Length; i++)
			{
				var c = instance[i];
				if (c == ' ' || c == '/' || c == '.') result += "-";
				else if (char.IsLetterOrDigit(c) || c == '-') result += GetUmlautAsciiFormat(c);
			}
			result = result.Replace("---", "-");
			result = result.Replace("--", "-");
			return result;
		}

		private static string GetUmlautAsciiFormat(this char instance)
		{
			var result = instance.ToString();

			if (instance == 'Ä') result = "Ae";
			else if (instance == 'ä') result = "ae";
			else if (instance == 'Ö') result = "Oe";
			else if (instance == 'ö') result = "oe";
			else if (instance == 'Ü') result = "Ue";
			else if (instance == 'ü') result = "ue";
			else if (instance == 'ß') result = "ss";

			return result;
		}

		#endregion

		#region Comparision & Finding

		public static bool EqualsSafe(this string instance, string compareTo)
		{
			if (instance == null) return compareTo == null;
			return instance.Equals(compareTo);
		}

		public static bool EqualsIgnoreCase(this string instance, string compareTo)
		{
			if (instance == null) return compareTo == null;
			return instance.Equals(compareTo, StringComparison.OrdinalIgnoreCase);
		}

		public static bool ContainsIgnoreCase(this string instance, string compareTo)
			=> instance.IndexOfWithIgnoreCase(compareTo) >= 0;

		public static bool ContainsAnyIgnoreCase(this string instance, IEnumerable<string> anyOf)
			=> anyOf.Any(x => instance.IndexOfWithIgnoreCase(x) >= 0);

		public static bool Contains(this string instance, string compareTo, StringComparison stringComparison)
			=> instance.IndexOf(compareTo, stringComparison) >= 0;

		public static bool StartsWithIgnoreCase(this string instance, string compareTo)
		{
			if (instance == null) return false;
			return instance.StartsWith(compareTo, StringComparison.OrdinalIgnoreCase);
		}

		public static bool EndsWithIgnoreCase(this string instance, string compareTo)
		{
			if (instance == null) return false;
			return instance.EndsWith(compareTo, StringComparison.OrdinalIgnoreCase);
		}

		public static int IndexOfWithIgnoreCase(this string instance, string compareTo, int startIndex = 0)
		{
			if (!instance.HasContent()) return -1;
			return instance.IndexOf(compareTo ?? "", startIndex, StringComparison.OrdinalIgnoreCase);
		}

		public static int NthIndexOfWithIgnoreCase(this string instance, int nth, string compareTo)
		{
			if (!instance.HasContent())
				return -1;

			var index = -1;
			var count = 0;

			while (true)
			{
				index = instance.IndexOfWithIgnoreCase(compareTo ?? "", index + 1);

				if (index < 0)
					return -1;

				if (++count == nth)
					return index;
			}
		}

		#endregion

		#region Replacement

		public static string ReplaceAll(this string instance, IEnumerable<string> strings, string newValue)
		{
			if (!instance.HasContent()) return instance;
			var result = instance;

			foreach (string s in strings)
				result = result.Replace(s, newValue);

			return result;
		}

		#endregion

		#region Conversions

		public static string Reverse(this string instance)
		{
			var arr = instance.ToCharArray();
			Array.Reverse(arr);
			return new string(arr);
		}

		public static Stream ToMemoryStream(this string instance)
		{
			var byteArray = Encoding.UTF8.GetBytes(instance);
			return new MemoryStream(byteArray);
		}

		public static byte[] ToByteArrayWithAscii(this string instance)
		{
			var enc = new ASCIIEncoding();
			return enc.GetBytes(instance);
		}

		private static void JsonErrorHandler(object x, Newtonsoft.Json.Serialization.ErrorEventArgs error)
			=> error.ErrorContext.Handled = true;

		private static DateTime? ToDate(this string instance)
		{
			if (!instance.HasContent())
				return null;

			var input = instance.ReplaceAll(new[] { "-", ":" }, ".");

			if (DateTime.TryParse(input, out DateTime result))
				return result;

			return null;
		}

		private static DateTime? GetDateTimeBySystemFormat(string instance)
		{
			if (!DateTime.TryParse(instance, out DateTime result))
				return null;

			return result;
		}

		private static DateTime? GetDateTimeByPlainInput(string instance)
		{
			var token = instance.SplitSafe(' ');
			var tokenCount = token.Count();
			if (tokenCount < 1 || tokenCount > 2) return null;

			var result = token.ElementAt(0).ToDate();
			if (result == null) return null;

			if (token.Count() > 1) result = result.Value.Add(token.ElementAt(1).ToTimeSpan());

			return result;
		}

		public static DateTime? ToDateTime(this string instance)
		{
			if (!instance.HasContent()) return null;

			var hasSystemFormat = instance.Contains("Z");

			var result =
				hasSystemFormat
					? GetDateTimeBySystemFormat(instance)
					: GetDateTimeByPlainInput(instance);

			return result;
		}

		private static DateTime? TryParseDate(
			string instance,
			string separator,
			int threeYearPos,
			int threeMonthPos,
			int threeDayPos,
			int twoMonthPos,
			int twoDayPos
		)
		{
			var token = instance.SplitSafe(separator);
			if (token.Count < 2) return null;
			if (token.Count > 3) return null;

			var year = DateTime.Now.Year;
			var month = 0;
			var day = 0;

			if (token.Count == 3)
			{
				year = token[threeYearPos].ToInt32();
				month = token[threeMonthPos].ToInt32();
				day = token[threeDayPos].ToInt32();
			}
			else
			{
				month = token[twoMonthPos].ToInt32();
				day = token[twoDayPos].ToInt32();
			}

			if (day < 1 || day > 31)
				return null;

			if (month < 1 || month > 12)
				return null;

			var result = new DateTime(year, month, day);
			return result;
		}

		private static DateTime? TryParseDateEu(string instance)
			=> TryParseDate(instance, ".", 2, 1, 0, 1, 0);

		private static DateTime? TryParseDateUs(string instance)
			=> TryParseDate(instance, "-", 0, 1, 2, 0, 1);

		public static DateTime? TryParseDate(this string instance)
		{
			if (!instance.HasContent())
				return null;

			if (instance.Contains("."))
				return TryParseDateEu(instance);

			return TryParseDateUs(instance);
		}

		public static TimeSpan ToTimeSpan(this string instance)
		{
			var hours = 0;
			var minutes = 0;
			var seconds = 0;

			instance = instance.ReplaceAll(new string[] { ".", "-" }, ":");

			var token = instance.SplitSafe(':');

			if (token.Count() > 0) hours = token.ElementAt(0).ToInt32();
			if (token.Count() > 1) minutes = token.ElementAt(1).ToInt32();
			if (token.Count() > 2) seconds = token.ElementAt(2).ToInt32();

			var result = new TimeSpan(hours, minutes, seconds);
			return result;
		}

		#endregion

		#region Substrings

		public static string SubstringSafe(this string instance, int startIndex, int length)
		{
			if (!instance.HasContent()) return instance;
			if (startIndex + length > instance.Length) return instance.Substring(startIndex);
			return instance.Substring(startIndex, length);
		}

		public static string Left(this string instance, int length)
		{
			if (instance == null) return null;
			if (length < 0) return instance.Substring(0, instance.Length + length);
			if (length > instance?.Length) return instance;

			return instance.Substring(0, length);
		}

		public static string Left(this string instance, string untilIndexOf)
		{
			if (!instance.HasContent()) return null;
			var index = instance.IndexOf(untilIndexOf);
			if (index < 0) return instance;
			return instance.Substring(0, index);
		}

		public static string Right(this string instance, int length)
		{
			if (instance == null) return null;
			if (length < 0) return instance.Substring(-length);
			if (length > instance?.Length) return instance;

			return instance.Substring(instance.Length - length);
		}

		#endregion

		#region XML

		private static Dictionary<Type, XmlSerializer> _XmlSerializerCache
			= new Dictionary<Type, XmlSerializer>();

		private static XmlSerializer GetOrCreateXmlSerializer<T>()
		{
			var type = typeof(T);
			if (!_XmlSerializerCache.ContainsKey(type))
				_XmlSerializerCache[type] = new XmlSerializer(type);

			return _XmlSerializerCache[type];
		}

		public static string ToXml<T>(
			this T instance,
			Action<XmlSerializerNamespaces> namespaceFactory = null
			)
		{
			if (instance == null) return null;
			var serializer = GetOrCreateXmlSerializer<T>();

			var xmlWriterSettings = new XmlWriterSettings
			{
				Encoding = Encoding.UTF8,
				Indent = true,
				ConformanceLevel = ConformanceLevel.Document,
				NamespaceHandling = NamespaceHandling.OmitDuplicates
			};

			using (var sw = new Utf8StringWriter())
			using (var writer = XmlWriter.Create(sw, xmlWriterSettings))
			{
				writer.WriteWhitespace("");

				var namespaces = new XmlSerializerNamespaces();
				namespaces.Add("", "");
				namespaceFactory?.Invoke(namespaces);

				serializer.Serialize(writer, instance, namespaces);
				var result = sw.ToString();
				return result;
			}
		}

		internal class Utf8StringWriter : StringWriter
		{
			public override Encoding Encoding => Encoding.UTF8;
		}

		public static T ToObjectByXml<T>(this string instance)
		{
			if (!instance.HasContent()) return default(T);
			var serializer = GetOrCreateXmlSerializer<T>();

			using (var sr = new StringReader(instance))
			{
				var result = (T)serializer.Deserialize(sr);
				return result;
			}
		}

		#endregion

		#region Split

		public static List<string> SplitSafe(
			this string instance,
			char separator = ',',
			bool removeEmptyEntries = true
			)
			=> SplitSafe(instance, separator.ToString(), removeEmptyEntries);

		public static List<string> SplitSafe(
			this string instance,
			string separator,
			bool removeEmptyEntries = true
			)
			=> SplitSafe(instance, new[] { separator }, removeEmptyEntries);

		public static List<string> SplitSafe(
			this string instance,
			IEnumerable<string> separator,
			bool removeEmptyEntries = true
			)
		{
			if (!instance.HasContent()) return new List<string>();

			var options =
				removeEmptyEntries
					? StringSplitOptions.RemoveEmptyEntries
					: StringSplitOptions.None;

			var charSeparator =
					separator
						.Select(x => x[0])
						.ToArray();

			return instance
				.Split(charSeparator, options)
				.Select(x => x.Trim())
				.ToList();
		}

		#endregion
	}
}
