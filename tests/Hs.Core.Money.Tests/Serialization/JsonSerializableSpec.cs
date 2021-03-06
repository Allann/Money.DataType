﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Hs.Core.Money.Extensions;
using FluentAssertions;
using Xunit;

namespace Hs.Core.Money.Serialization.Tests
{
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
                new object[] { "{ value: 200 }" },
            };

            public static IEnumerable<object[]> ValidNestedJsonData => new[]
            {
                new object[] { $"{{ cash: {{ value: '200', currency: '{_currentCultureCode}' }} }}", },
                new object[] { $"{{ cash: {{ value: 200, currency: '{_currentCultureCode}' }} }}" },
                new object[] { $"{{ cash: {{ currency: '{_currentCultureCode}', value: 200 }} }}" },
                new object[] { $"{{ cash: {{ currency: '{_currentCultureCode}', value: '200' }} }}" }
            };

            public static IEnumerable<object[]> ValidNestedNullableJsonData => new[]
            {
                new object[] { $"{{ cash: {{ value: '200', currency: '{_currentCultureCode}' }} }}", },
                new object[] { $"{{ cash: {{ value: 200, currency: '{_currentCultureCode}' }} }}" },
                new object[] { $"{{ cash: {{ currency: '{_currentCultureCode}', value: 200 }} }}" },
                new object[] { $"{{ cash: {{ currency: '{_currentCultureCode}', value: '200' }} }}" },
                new object[] { $"{{ cash: null }}" },
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
                a.Should().Throw<ArgumentNullException>();
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
}
