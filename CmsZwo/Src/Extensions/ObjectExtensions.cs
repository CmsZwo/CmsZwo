using System;
using System.Globalization;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace CmsZwo
{
	public static class ObjectExtensions
	{
		#region Conversions

		public static int ToInt32(this object instance, int onFail = 0)
		{
			if (instance == null)
				return onFail;

			if (instance is int)
				return (int)instance;

			if (instance is float f)
				return (int)Math.Round(f);

			if (instance is double d)
				return (int)Math.Round(d);

			if (int.TryParse(instance.ToString(), out int result))
				return result;

			return onFail;
		}

		public static bool ToBoolean(this object instance, bool onFail = false)
		{
			if (instance == null)
				return onFail;

			if (instance is bool)
				return (bool)instance;

			var str = instance.ToString();
			if (str.EqualsIgnoreCase("true") || str == "1")
				return true;

			if (str.EqualsIgnoreCase("false") || str == "0")
				return false;

			return onFail;
		}

		public static double ToDouble(this object instance, double onFail = 0d)
		{
			if (instance == null)
				return onFail;

			if (instance is double)
				return (double)instance;

			if (double.TryParse(instance.ToString(), out double result))
				return result;

			return onFail;
		}

		public static float ToFloat(this object instance, float onFail = 0f)
		{
			if (instance == null)
				return onFail;

			if (instance is float)
				return (float)instance;

			if (float.TryParse(instance.ToString(), out float result))
				return result;

			return onFail;
		}

		public static T ToEnum<T>(this object instance, T onFail = default(T))
			where T : struct, IConvertible
		{
			if (instance == null)
				return onFail;

			if (Enum.TryParse(instance.ToString(), out T result))
				return result;

			return onFail;
		}

		private static readonly JsonSerializerSettings _CamelCaseSettings
			= new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

		public static string ToJson(this object instance, bool forceCamelCase = false)
		{
			if (!forceCamelCase)
				return JsonConvert.SerializeObject(instance);

			return JsonConvert.SerializeObject(instance, Formatting.None, _CamelCaseSettings);
		}

		private static readonly JsonSerializerSettings _TypeInformationSettings
			= new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

		public static string ToJsonWithTypeInformation(this object instance)
			=> JsonConvert.SerializeObject(instance, Formatting.None, _TypeInformationSettings);

		public static string ToJsonMd5(this object instance)
			=> instance?.ToJson().ToMd5Hash();

		#endregion

		#region Serializer Settings

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

		private class DoubleSerializer : JsonConverter
		{
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				var doubleValue = value.ToDouble().ToString("#.#").Replace(NumberDecimalSeparator, ".");
				serializer.Serialize(writer, doubleValue);
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				if (objectType == typeof(double) || objectType == typeof(double?))
				{
					object result = null;

					var jsonObject = JValue.Load(reader);
					var stringValue = jsonObject.ToString();
					if (stringValue.HasContent())
					{
						if (!stringValue.IsNumeric())
						{
							var isNullable = objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>);
							if (isNullable) return null;
						}
						else
						{
							result = stringValue.ToDouble();
						}
					}

					return result;
				}
				else
				{
					reader.Skip();
				}
				return null;
			}

			public override bool CanConvert(Type objectType)
			{
				var result = objectType == typeof(double) || objectType == typeof(double?);
				return result;
			}
		}

		private class FloatSerializer : JsonConverter
		{
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				var floatValue = value.ToFloat().ToString("#.#").Replace(NumberDecimalSeparator, ".");
				serializer.Serialize(writer, floatValue);
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				if (objectType == typeof(float) || objectType == typeof(float?)
					 || objectType == typeof(Single) || objectType == typeof(Single?))
				{
					object result = null;

					var jsonObject = JToken.Load(reader);
					var stringValue = jsonObject.ToString();
					if (stringValue.HasContent())
					{
						if (!stringValue.IsNumeric())
						{
							var isNullable = objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>);
							if (isNullable) return null;
						}
						else
						{
							result = stringValue.ToFloat();
						}
					}

					return result;
				}
				else
				{
					reader.Skip();
				}
				return null;
			}

			public override bool CanConvert(Type objectType)
			{
				var result = objectType == typeof(float) || objectType == typeof(float?)
					 || objectType == typeof(Single) || objectType == typeof(Single?);
				return result;
			}
		}

		private class IntSerializer : JsonConverter
		{
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				var intValue = value.ToInt32();
				serializer.Serialize(writer, intValue);
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				if (objectType == typeof(int) || objectType == typeof(int?))
				{
					object result = null;

					var jsonObject = JToken.Load(reader);
					var stringValue = jsonObject.ToString();
					if (stringValue.HasContent())
					{
						if (!stringValue.IsNumeric())
						{
							var isNullable = objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>);
							if (isNullable) return null;
						}
						else
						{
							result = stringValue.ToInt32();
						}
					}

					return result;
				}
				else
				{
					reader.Skip();
				}
				return null;
			}

			public override bool CanConvert(Type objectType)
			{
				var result = objectType == typeof(int) || objectType == typeof(int?); ;
				return result;
			}
		}

		private static JsonSerializerSettings _JsonSerializerSettings = null;
		private static JsonSerializerSettings GetJsonSerializerSettings()
		{
			if (_JsonSerializerSettings != null) return _JsonSerializerSettings;

			_JsonSerializerSettings = new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Error,
				DateTimeZoneHandling = DateTimeZoneHandling.Local,
				Converters = new List<JsonConverter>() { new FloatSerializer(), new DoubleSerializer(), new IntSerializer() },
				ObjectCreationHandling = ObjectCreationHandling.Replace,
				NullValueHandling = NullValueHandling.Include,
				TypeNameHandling = TypeNameHandling.Auto
			};

			return _JsonSerializerSettings;
		}

		#endregion

		#region Create, Equals, Copy

		public static T ToObjectByJson<T>(this string instance)
		{
			if (!instance.HasContent() || instance == "[]") return default(T);
			var settings = GetJsonSerializerSettings();
			var result = JsonConvert.DeserializeObject<T>(instance, settings);
			return result;
		}

		public static T CopyByJson<T>(this T instance)
		{
			if (instance == null) return default(T);

			var result = default(T);
			var sourceJson = instance.ToJsonWithTypeInformation();

			var type = typeof(T);
			if (type.IsAbstract || type.IsInterface)
			{
				var method = typeof(ObjectExtensions).GetMethod(nameof(ToObjectByJson), new[] { typeof(string) });
				var instanceType = instance.GetType();
				var genericMethod = method.MakeGenericMethod(new[] { instanceType });
				result = (T)genericMethod.Invoke(null, new object[] { sourceJson });
			}
			else
			{
				result = sourceJson.ToObjectByJson<T>();
			}

			return result;
		}

		public static bool EqualsByJson(this object instance, object compareToObject)
		{
			var instanceJson = instance.ToJson();
			var compareJson = compareToObject.ToJson();
			var result = instanceJson == compareJson;
			return result;
		}

		#endregion

	}
}