using System.Globalization;
using AwesomeAssertions;
using Xunit;
using System.Xml.Linq;
using Hs.Core.Money.Extensions;

namespace Hs.Core.Money.Tests.CurrencySpec;

public class GivenIWantToKnowAllCurrencies
{
    [Fact]
    public void WhenAskingForIt_ThenAllCurrenciesShouldBeReturned()
    {
        var currencies = Currency.GetAllCurrencies();

        currencies.Should().NotBeEmpty();
        currencies.Count().Should().BeGreaterThan(100);
    }
}

public class GivenIWantCurrencyFromIsoCode
{
    [Fact]
    public void WhenIsoCodeIsExisting_ThenCreatingShouldSucceed()
    {
        var currency = Currency.FromCode("EUR");

        currency.Should().NotBeNull();
        currency.Symbol.Should().Be("€");
        currency.Code.Should().Be("EUR");
        currency.EnglishName.Should().Be("Euro");
        currency.IsValid.Should().BeTrue();
    }

    [Fact]
    public void WhenIsoCodeIsUnknown_ThenCreatingShouldThrow()
    {
        Action action = () => Currency.FromCode("AAA");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WhenIsoCodeIsNull_ThenCreatingShouldThrow()
    {
        Action action = () => Currency.FromCode(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WhenEstionianKrone_ThenItShouldBeObsolete()
    {
        var currency = Currency.FromCode("EEK");

        currency.Should().NotBeNull();
        currency.Symbol.Should().Be("kr");
        currency.IsValid.Should().BeFalse();
    }

}

public class GivenIWantCurrencyFromRegionOrCulture
{
    [Fact]
    public void WhenUsingRegionInfo_ThenCreatingShouldSucceed()
    {
        var currency = Currency.FromRegion(new RegionInfo("NL"));

        currency.Should().NotBeNull();
        currency.Symbol.Should().Be("€");
        currency.Code.Should().Be("EUR");
        currency.EnglishName.Should().Be("Euro");
    }

    [Fact]
    public void WhenRegionInfoIsNull_ThenCreatingShouldThrow()
    {
        Action action = () => Currency.FromRegion((RegionInfo)null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WhenUsingRegionName_ThenCreatingShouldSucceed()
    {
        var currency = Currency.FromRegion("NL");

        currency.Should().NotBeNull();
        currency.Symbol.Should().Be("€");
        currency.Code.Should().Be("EUR");
        currency.EnglishName.Should().Be("Euro");
    }

    [Fact]
    public void WhenUsingRegionNameThatIsNull_ThenCreatingShouldThrow()
    {
        Action action = () => Currency.FromRegion((string)null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WhenUsingCultureInfo_ThenCreatingShouldSucceed()
    {
        var currency = Currency.FromCulture(CultureInfo.CreateSpecificCulture("nl-NL"));

        currency.Should().NotBeNull();
        currency.Symbol.Should().Be("€");
        currency.Code.Should().Be("EUR");
        currency.EnglishName.Should().Be("Euro");
    }

    [Fact]
    public void WhenCultureInfoIsNull_ThenCreatingShouldThrow()
    {
        Action action = () => Currency.FromCulture(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WhenCultureInfoIsNeutralCulture_ThenCreatingShouldThrow()
    {
        Action action = () => Currency.FromCulture(new CultureInfo("en"));

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WhenUsingCultureName_ThenCreatingShouldSucceed()
    {
        var currency = Currency.FromRegion("nl-NL");

        currency.Should().NotBeNull();
        currency.Symbol.Should().Be("€");
        currency.Code.Should().Be("EUR");
        currency.EnglishName.Should().Be("Euro");
    }

    [Fact]
    public void WhenUsingCurrentCurrency_ThenCreatingShouldSucceed()
    {
        var currency = Currency.CurrentCurrency;

        currency.Should().Be(Currency.FromRegion(RegionInfo.CurrentRegion));
    }
}

public class GivenIWantToCompareCurrencies
{
    private readonly Currency _euro1 = Currency.FromCode("EUR");

    private readonly Currency _euro2 = Currency.FromCode("EUR");

    private readonly Currency _dollar = Currency.FromCode("USD");

    [Fact]
    public void WhenComparingEquality_ThenCurrencyShouldBeEqual()
    {
        // Compare using Equal()
        _euro1.Should().Be(_euro2);
        _euro1.Should().NotBe(_dollar);
        _euro1.Should().NotBeNull();
        _euro1.Should().NotBe(new object(), "comparing Currency to a different object should fail.");
    }

    [Fact]
    public void WhenComparingStaticEquality_ThenCurrencyShouldBeEqual()
    {
        // Compare using static Equal()
        Currency.Equals(_euro1, _euro2).Should().BeTrue();
        Currency.Equals(_euro1, _dollar).Should().BeFalse();
    }

    [Fact]
    public void WhenComparingWithEqualityOperator_ThenCurrencyShouldBeEqual()
    {
        // Compare using Euality operators
        (_euro1 == _euro2).Should().BeTrue();
        (_euro1 != _dollar).Should().BeTrue();
    }

    [Fact]
    public void WhenComparingHashCodes_ThenCurrencyShouldBeEqual()
    {
        // Compare using GetHashCode()
        _euro1.GetHashCode().Should().Be(_euro2.GetHashCode());
        _euro1.GetHashCode().Should().NotBe(_dollar.GetHashCode());
    }
}

public class GivenIWantToKnowMinorUnit
{
    private readonly Currency _eur = Currency.FromCode("EUR");

    private readonly Currency _yen = Currency.FromCode("JPY");

    private readonly Currency _din = Currency.FromCode("BHD");

    private readonly Currency _mga = Currency.FromCode("MGA"); // Malagasy ariary

    private readonly Currency _xau = Currency.FromCode("XAU"); // Gold

    [Fact]
    public void WhenAskingForEuro_ThenMinorUnitShouldBeOneCent()
    {
        _eur.MajorUnit.Should().Be(1m);
        _eur.MinorUnit.Should().Be(0.01m);
        _eur.DecimalDigits.Should().Be(2);
    }

    [Fact]
    public void WhenAskingForYen_ThenMinorUnitShouldBeOne()
    {
        _yen.MajorUnit.Should().Be(1m);
        _yen.MinorUnit.Should().Be(1m);
        _yen.DecimalDigits.Should().Be(0);
    }

    [Fact]
    public void WhenAskingForDinar_ThenMinorUnitShouldBeOneFils()
    {
        _din.MajorUnit.Should().Be(1m);
        _din.MinorUnit.Should().Be(0.001m);
        _din.DecimalDigits.Should().Be(3);
    }

    [Fact]
    public void WhenAskingForGold_ThenMinorUnitShouldBeOne()
    {
        _xau.MajorUnit.Should().Be(1m);
        _xau.MinorUnit.Should().Be(1m);
        _xau.DecimalDigits.Should().Be(0); // ISO 4217 lists gold without minor units ("N.A.")
    }

    [Fact]
    public void WhenAskingForMalagasyAriary_ThenMinorUnitShouldBeOneFith()
    {
        // The Malagasy ariary are technically divided into five subunits, where the coins display "1/5" on their face and
        // are referred to as a "fifth"; These are not used in practice, but when written out, a single significant digit
        // is used. E.g. 1.2 UM.
        _mga.MajorUnit.Should().Be(1m);
        _mga.MinorUnit.Should().Be(0.2m);
        _mga.DecimalDigits.Should().Be(1);
    }
}

public class GivenIWantToInitiateInternallyACurrency
{
    [Fact]
    public void WhenParamsAreCorrect_ThenCreatingShouldSucceed()
    {
        var eur = new Currency("EUR", "978", 2, "Euro", "€");

        eur.Code.Should().Be("EUR");
        eur.Number.Should().Be("978");
        eur.DecimalDigits.Should().Be(2);
        eur.EnglishName.Should().Be("Euro");
        eur.Symbol.Should().Be("€");
    }

    [Fact]
    public void WhenCodeIsNull_ThenCreatingShouldThrow()
    {
        Action action = () => { _ = new Currency(null!, "978", 2, "Euro", "€"); };

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WhenNumberIsNull_ThenNumberShouldDefaultToEmpty()
    {
        var eur = new Currency("EUR", null, 2, "Euro", "€");

        eur.Number.Should().Be(string.Empty);
    }

    [Fact]
    public void WhenEnglishNameIsNull_ThenEnglishNameShouldDefaultToEmpty()
    {
        var eur = new Currency("EUR", "978", 2, null, "€");

        eur.EnglishName.Should().Be(string.Empty);
    }

    [Fact]
    public void WhenSignIsNull_ThenSignShouldDefaultToGenericCurrencySign()
    {
        var eur = new Currency("EUR", "978", 2, "Euro", null);

        eur.Symbol.Should().Be(Currency.GenericCurrencySign);
    }

    [Fact]
    public void WhenDecimalDigitIsLowerThenMinusOne_ThenCreatingShouldThrow()
    {
        Action action = () => { _ = new Currency("EUR", "978", -2, "Euro", "€"); };

        action.Should().Throw<ArgumentOutOfRangeException>();
    }
}

public class GivenIWantToSerializeCurrencyWithJsonSerializer
{
    [Fact]
    public void WhenSerializingYen_ThenThisShouldSucceed()
    {
        var yen = Currency.FromCode("JPY");
        Clone<Currency>(yen).Should().Be(yen);
    }

    [Fact]
    public void WhenSerializingEuro_ThenThisShouldSucceed()
    {
        var euro = Currency.FromCode("EUR");
        Clone<Currency>(euro).Should().Be(euro);
    }

    private static T Clone<T>(object source)
    {
        return source.ToJson().FromJson<T>()!;
    }
}

public class GivenIWantToDeconstructCurrency
{
    [Fact]
    public void WhenDeconstructing_ThenShouldSucceed()
    {
        var currency = Currency.FromCode("EUR");

        var (code, number, symbol) = currency;

        code.Should().Be("EUR");
        number.Should().Be("978");
        symbol.Should().Be("€");
    }
}

public class GivenIWantToValidateTheDateRange
{
    [Fact]
    public void WhenValidatingACurrencyThatIsAlwaysValid_ThenShouldSucceed()
    {
        var currency = Currency.FromCode("EUR");

        currency.ValidFrom.Should().BeNull();
        currency.ValidTo.Should().BeNull();

        currency.IsValidOn(DateTime.Today).Should().BeTrue();
    }

    [Fact]
    public void WhenValidatingACurrencyThatIsValidUntilACertainDate_ThenShouldBeValidStrictlyBeforeThatDate()
    {
        var currency = Currency.FromCode("VEB");

        currency.ValidFrom.Should().BeNull();
        currency.ValidTo.Should().Be(new DateTime(2008, 1, 1));

        currency.IsValidOn(DateTime.MinValue).Should().BeTrue();
        currency.IsValidOn(DateTime.MaxValue).Should().BeFalse();
        currency.IsValidOn(new DateTime(2007, 12, 31)).Should().BeTrue();
        // assumes that the until date given in the wikipedia article is excluding.
        // assumption based on the fact that some dates are the first of the month/year
        // and that the euro started at 1999-01-01. Given that the until date of e.g. the Dutch guilder
        // is 1999-01-01, the until date must be excluding
        currency.IsValidOn(new DateTime(2008, 1, 1)).Should().BeTrue("the until date is excluding");
    }

    [Fact]
    public void WhenValidatingACurrencyThatIsValidFromACertainDate_ThenShouldBeValidFromThatDate()
    {
        var currency = Currency.FromCode("VES");

        currency.ValidFrom.Should().Be(new DateTime(2018, 8, 20));
        currency.ValidTo.Should().BeNull();

        currency.IsValidOn(DateTime.MinValue).Should().BeFalse();
        currency.IsValidOn(DateTime.MaxValue).Should().BeTrue();
        currency.IsValidOn(new DateTime(2018, 8, 19)).Should().BeFalse();
        currency.IsValidOn(new DateTime(2018, 8, 20)).Should().BeTrue();
    }
}

/// <summary>The currency table agrees with the official ISO 4217 list (a snapshot in Data/iso-4217-list-one.xml).</summary>
/// <remarks>Refresh the snapshot from https://www.six-group.com/en/products-services/financial-information/data-standards.html.</remarks>
public class GivenTheOfficialIsoList
{
    // ISO lists these as 2 minor units; they're really divided into fifths, see Currency.MinorUnit.
    private static readonly string[] DividedIntoFifths = ["MGA", "MRU"];

    private static readonly XDocument Snapshot = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "Data", "iso-4217-list-one.xml"));

    private static readonly DateTime Published = DateTime.Parse(Snapshot.Root!.Attribute("Pblshd")!.Value, CultureInfo.InvariantCulture);

    private static readonly IReadOnlyList<IsoCurrency> Iso =
        Snapshot.Descendants("CcyNtry")
            .Where(e => e.Element("Ccy") is not null)
            .Select(e => new IsoCurrency((string)e.Element("Ccy")!, (string?)e.Element("CcyNbr") ?? "", (string?)e.Element("CcyMnrUnts") ?? ""))
            .DistinctBy(c => c.Code)
            .ToList();

    // The shipped table, not the live registry, which other tests register currencies in.
    private static readonly IReadOnlyList<Currency> Defined =
        DefaultCurrencies.All.Where(c => c.Namespace == "ISO-4217").ToList();

    [Fact]
    public void WhenACurrencyIsInTheIsoList_ThenItIsDefined() =>
        Iso.Select(c => c.Code).Except(Defined.Select(c => c.Code)).Should().BeEmpty();

    [Fact]
    public void WhenACurrencyIsDefinedAndCurrent_ThenItIsInTheIsoList() =>
        Defined.Where(c => c.IsValidOn(Published)).Select(c => c.Code).Except(Iso.Select(c => c.Code)).Should().BeEmpty();

    [Fact]
    public void WhenACurrencyIsInBoth_ThenItHasTheSameNumber() =>
        Iso.Join(Defined, iso => iso.Code, defined => defined.Code, (iso, defined) => (iso.Code, Iso: iso.Number, Defined: defined.Number))
            .Where(c => c.Iso != c.Defined)
            .Should().BeEmpty();

    [Fact]
    public void WhenACurrencyIsInBoth_ThenItHasTheSameMinorUnits() =>
        Iso.Where(iso => !DividedIntoFifths.Contains(iso.Code))
            .Join(Defined, iso => iso.Code, defined => defined.Code, (iso, defined) => (iso.Code, Iso: iso.MinorUnits, Defined: defined.DecimalDigits))
            .Where(c => (c.Iso == "N.A." ? 0 : int.Parse(c.Iso, CultureInfo.InvariantCulture)) != c.Defined)
            .Should().BeEmpty();

    private sealed record IsoCurrency(string Code, string Number, string MinorUnits);
}
