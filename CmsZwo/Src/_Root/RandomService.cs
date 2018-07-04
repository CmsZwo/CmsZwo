using System;
using System.Linq;
using System.Security.Cryptography;

namespace CmsZwo
{
	public interface IRandomService : IService
	{
		int Random(int min, int max);
		string RandomBase24(int length, bool uppercase = false);
	}

	public class RandomService : IRandomService
	{
		#region Construct

		private readonly char[] _HumanDistinctableCharacterSet;
		private readonly char[] _HumanDistinctableCharacterSetUppercase;

		public RandomService()
		{
			_HumanDistinctableCharacterSet = new char[] { 'A', 'a', 'd', 'E', 'e', 'F', 'G', 'g', 'H', 'h', 'M', 'm', 'N', 'n', 'R', 'T', 't', '2', '3', '4', '5', '7', '8', '9' };
			_HumanDistinctableCharacterSetUppercase = _HumanDistinctableCharacterSet.Select(x => x.ToString().ToUpper()[0]).Distinct().ToArray();
		}

		#endregion

		#region Human Distinctable

		public string RandomBase24(int length, bool uppercase = false)
		{
			var result = "";

			var characterSet = uppercase ? _HumanDistinctableCharacterSetUppercase : _HumanDistinctableCharacterSet;

			int max = characterSet.Length;
			char lastCharacter = ' ';
			char newCharacter;

			for (int i = 0; i < length; i++)
			{
				do
				{
					var randomNumber = Random(0, max);
					newCharacter = characterSet[randomNumber];
				}
				while (lastCharacter.ToString().ToUpper() == newCharacter.ToString().ToUpper());

				lastCharacter = newCharacter;
				result += newCharacter;
			}

			return result;
		}

		#endregion

		#region Numbers

		private RNGCryptoServiceProvider _RNGCryptoServiceProvider = null;
		public int Random(int min, int max)
		{
			if (_RNGCryptoServiceProvider == null)
				_RNGCryptoServiceProvider = new RNGCryptoServiceProvider();

			var randomNumber = new byte[4];
			_RNGCryptoServiceProvider.GetBytes(randomNumber);
			var result = Math.Abs(BitConverter.ToInt32(randomNumber, 0));
			return result % max + min;
		}

		#endregion
	}
}
