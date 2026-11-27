using System.Globalization;
using Hs.Core.Money.Extensions;
using AwesomeAssertions;
using Xunit;

namespace Hs.Core.Money.Serialization.Tests;

public static class JsonSerializableSpec
{
    public class GivenIWantToSerializeAmounWithJsonSerializer
    {
        public static IEnumerable<object[]> TestData => new[]
        {
            new object[] { new Amount(765m, Currency.FromCode("JPY")) },
            new object[] { new Amount(765.43m, Currency.FromCode("EUR")) }
        };

        [Theory]
        [MemberData(nameof(TestData))]
        public void WhenSerializing_ThenThisShouldSucceed(Amount money)
        {
            var json = money.ToJson();
            var clone = json.FromJson<Amount>();

            clone.Should().Be(money);
        }

        [Fact]
        public void WhenUsingSystemTextJsonDirectly_ThenThisShouldSucceed()
        {
            var money = new Amount(765.43m, Currency.FromCode("EUR"));
            var json = System.Text.Json.JsonSerializer.Serialize(money);
            var clone = System.Text.Json.JsonSerializer.Deserialize<Amount>(json);

            clone.Should().Be(money);
        }
    }

    public class GivenIWantToDeserializeAmountWithJsonSerializer
    {
        private static readonly string _currentCultureCode = new RegionInfo(CultureInfo.CurrentCulture.LCID).ISOCurrencySymbol;

        public static IEnumerable<object[]> ValidJsonData => new[]
        {
            new object[] { $"{{ \"value\": 200, \"currency\": \"{_currentCultureCode}\" }}" },
            new object[] { $"{{ \"currency\": \"{_currentCultureCode}\", \"value\": 200 }}" },
        };

        public static IEnumerable<object[]> InvalidJsonData => new[]
        {
            new object[] { "{ value: '200' }" },
        };

        public static IEnumerable<object[]> ValidNestedJsonData => new[]
        {
            new object[] { $"{{ \"cash\": {{ \"value\": \"200\", \"currency\": \"{_currentCultureCode}\" }} }}", },
            new object[] { $"{{ \"cash\": {{ \"value\": 200, \"currency\": \"{_currentCultureCode}\" }} }}" },
            new object[] { $"{{ \"cash\": {{ \"currency\": \"{_currentCultureCode}\", \"value\": 200 }} }}" },
            new object[] { $"{{ \"cash\": {{ \"currency\": \"{_currentCultureCode}\", \"value\": \"200\" }} }}" }
        };

        public static IEnumerable<object[]> ValidNestedNullableJsonData => new[]
        {
            new object[] { $"{{ \"cash\": {{ \"value\": \"200\", \"currency\": \"{_currentCultureCode}\" }} }}", },
            new object[] { $"{{ \"cash\": {{ \"value\": 200, \"currency\": \"{_currentCultureCode}\" }} }}" },
            new object[] { $"{{ \"cash\": {{ \"currency\": \"{_currentCultureCode}\", \"value\": 200 }} }}" },
            new object[] { $"{{ \"cash\": {{ \"currency\": \"{_currentCultureCode}\", \"value\": \"200\" }} }}" },
            new object[] { "{ \"cash\": null }" },
        };

        [Theory]
        [MemberData(nameof(ValidJsonData))]
        public void WhenDeserializingWithValidJson_ThenThisShouldSucceed(string json)
        {
            var money = new Amount(200, Currency.FromCode(_currentCultureCode));

            var clone = json.FromJson<Amount>();

            clone.Should().Be(money);
        }

        [Theory]
        [MemberData(nameof(InvalidJsonData))]
        public void WhenDeserializingWithInvalidJson_ThenThisShouldFail(string json)
        {
            Action a = () => json.FromJson<Amount>();
            a.Should().Throw<System.Text.Json.JsonException>();
        }

        [Fact]
        public void WhenDeserializingWithoutCurrency_ThenThisShouldFail()
        {
            Action action = () => "{\"value\":200}".FromJson<Amount>();
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenExcludingNullProperties_ThenNullPropertiesShouldNotBeSerialized()
        {
            var item = new TypeWithNullableMoneyProperty { Cash = null };

            item.ToJson(includeNull: false).Should().NotContain("cash");
            item.ToJson().Should().Contain("\"cash\":null");
        }

        private class TypeWithMoneyProperty
        {
            public Amount Cash { get; set; }
        }

        [Theory]
        [MemberData(nameof(ValidNestedJsonData))]
        public void WhenDeserializingWithNested_ThenThisShouldSucceed(string json)
        {
            var money = new Amount(200, Currency.FromCode(_currentCultureCode));

            var clone = json.FromJson<TypeWithMoneyProperty>();

            clone.Cash.Should().Be(money);
        }

        private class TypeWithNullableMoneyProperty
        {
            public Amount? Cash { get; set; }
        }

        [Theory]
        [MemberData(nameof(ValidNestedNullableJsonData))]
        public void WhenDeserializingWithNestedNullable_ThenThisShouldSucceed(string json)
        {
            var money = new Amount(200, Currency.FromCode(_currentCultureCode));

            var clone = json.FromJson<TypeWithNullableMoneyProperty>();

            if (!json.Contains("null"))
            {
                clone.Cash.Should().Be(money);
            }
            else
            {
                clone.Cash.Should().BeNull();
            }
        }
    }
}
