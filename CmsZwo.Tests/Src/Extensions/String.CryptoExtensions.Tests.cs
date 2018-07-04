using Xunit;

namespace CmsZwo.Extensions.Tests
{
	public class StringCryptoTests
	{
		[Fact]
		public void CreateSaltMd5()
		{
			var saltMd5 = "Salt".CreateSaltMd5();
			Assert.Equal("jrwwp0fl7U/0BNZlEjZz4A1Ta57DIKRYcA==", saltMd5);
		}

		[Fact]
		public void HashPasswordMd5()
		{
			var saltMd5 = "Salt".CreateSaltMd5();
			Assert.Equal("O+7cK6mtlxJCsPeKY5iCDUKKkc6qHPRRng==", "Password".HashPasswordMd5(saltMd5));
		}

		[Fact]
		public void PasswordMd5Match()
		{
			Assert.True("Password".PasswordMd5Match("jrwwp0fl7U/0BNZlEjZz4A1Ta57DIKRYcA==", "O+7cK6mtlxJCsPeKY5iCDUKKkc6qHPRRng=="));
		}

		[Fact]
		public void HashWithSha512()
		{
			Assert.Equal("0ad64c08fd21275f28706eb845bbe5953c6a5f0e641230f7880a517a0bcfdb15ab54edbef8f624263b08cb778d04389e8a682c0416c67e1c4e8b3d4fc4120675", "Secret".HashWithSha512());
		}

		[Fact]
		public void ValidateWithSha512()
		{
			Assert.True("0ad64c08fd21275f28706eb845bbe5953c6a5f0e641230f7880a517a0bcfdb15ab54edbef8f624263b08cb778d04389e8a682c0416c67e1c4e8b3d4fc4120675".ValidateWithSha512("Secret"));
		}

		[Fact]
		public void HashWithSha1()
		{
			Assert.Equal("f4e7a8740db0b7a0bfd8e63077261475f61fc2a6", "Secret".HashWithSha1());
		}

		[Fact]
		public void ValidateWithSha1()
		{
			Assert.True("f4e7a8740db0b7a0bfd8e63077261475f61fc2a6".ValidateWithSha1("Secret"));
		}

		[Fact]
		public void ToMd5Hash()
		{
			Assert.Equal("1e6947ac7fb3a9529a9726eb692c8cc5", "Secret".ToMd5Hash());
		}
	}
}
