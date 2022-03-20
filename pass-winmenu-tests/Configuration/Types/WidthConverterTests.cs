using System.Windows;
using PassWinmenu.Configuration.Types;
using PassWinmenuTests.Utilities;
using Shouldly;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace PassWinmenuTests.Configuration.Types
{
	public class WidthConverterTests
	{
		[Theory]
		[InlineData("0", 0)]
		[InlineData("1.77", 1.77)]
		[InlineData("1.2e-3", 1.2e-3)]
		public void Deserialize_FromDouble_UniformThickness(string thickness, double expected)
		{
			var des = GetDeserialiser();

			var obj = des.Deserialize<WrapperObject>("Border: " + thickness);

			obj.Border.ShouldSatisfyAllConditions(
				() => obj.Border.Top.ShouldBe(expected),
				() => obj.Border.Left.ShouldBe(expected),
				() => obj.Border.Bottom.ShouldBe(expected),
				() => obj.Border.Right.ShouldBe(expected));
		}

		[Theory]
		[InlineData("text")]
		public void Deserialize_InvalidInput_ThrowsException(string thickness)
		{
			var des = GetDeserialiser();

			Should.Throw<YamlException>(() => des.Deserialize<WrapperObject>("Border: " + thickness));
		}

		[Fact]
		public void Deserialize_ArrayOf0_UniformZeroThickness()
		{
			const double expected = 0;
			var des = GetDeserialiser();

			var obj = des.Deserialize<WrapperObject>("Border: []");

			obj.Border.ShouldSatisfyAllConditions(
				() => obj.Border.Top.ShouldBe(expected),
				() => obj.Border.Left.ShouldBe(expected),
				() => obj.Border.Bottom.ShouldBe(expected),
				() => obj.Border.Right.ShouldBe(expected));
		}

		[Fact]
		public void Deserialize_ArrayOf1_UniformThickness()
		{
			const double expected = 2.2;
			var des = GetDeserialiser();

			var obj = des.Deserialize<WrapperObject>("Border: [2.2]");

			obj.Border.ShouldSatisfyAllConditions(
				() => obj.Border.Top.ShouldBe(expected),
				() => obj.Border.Left.ShouldBe(expected),
				() => obj.Border.Bottom.ShouldBe(expected),
				() => obj.Border.Right.ShouldBe(expected));
		}

		[Fact]
		public void Deserialize_ArrayOf2_UniformParallelThickness()
		{
			const double expectedX = 1;
			const double expectedY = 4.17;
			var des = GetDeserialiser();

			var obj = des.Deserialize<WrapperObject>("Border: [1, 4.17]");

			obj.Border.ShouldSatisfyAllConditions(
				() => obj.Border.Top.ShouldBe(expectedX),
				() => obj.Border.Left.ShouldBe(expectedY),
				() => obj.Border.Bottom.ShouldBe(expectedX),
				() => obj.Border.Right.ShouldBe(expectedY));
		}

		[Fact]
		public void Deserialize_ArrayOf3_ThrowsException()
		{
			var des = GetDeserialiser();

			Should.Throw<YamlException>(() => des.Deserialize<WrapperObject>("Border: [1, 1, 1]"));
		}

		[Fact]
		public void Deserialize_ArrayOf4_ThicknessAsSpecified()
		{
			var des = GetDeserialiser();

			var obj = des.Deserialize<WrapperObject>("Border: [1, -2.22, 300, 5]");

			obj.Border.ShouldSatisfyAllConditions(
				() => obj.Border.Top.ShouldBe(1),
				() => obj.Border.Left.ShouldBe(5),
				() => obj.Border.Bottom.ShouldBe(300),
				() => obj.Border.Right.ShouldBe(-2.22));
		}


		private class WrapperObject
		{
			public Thickness Border { get; set; }
		}

		private static IDeserializer GetDeserialiser()
		{
			return new DeserializerBuilder()
				.WithTypeConverter(new WidthConverter())
				.Build();
		}
	}
}
