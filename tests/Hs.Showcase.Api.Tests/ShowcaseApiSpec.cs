using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Hs.Showcase.Api.Tests;

/// <summary>The showcase API over HTTP, end to end through ASP.NET Core's pipeline.</summary>
public sealed class ShowcaseApiSpec(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    [Fact]
    public async Task ScalarApiReferenceIsAvailableInDevelopment()
    {
        var response = await _client.GetAsync("/scalar", Cancellation);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OpenApiDocumentListsTheShowcaseEndpoints()
    {
        using var document = await _client.GetFromJsonAsync<JsonDocument>("/openapi/v1.json", Cancellation);
        var paths = document!.RootElement.GetProperty("paths");

        paths.EnumerateObject().Select(path => path.Name).Should().Contain(
            ["/api/languages", "/api/currencies", "/api/words", "/api/amount", "/api/divide"]);
    }

    [Fact]
    public async Task OpenApiDocumentsEndpointAndQueryParameterDescriptions()
    {
        using var document = await _client.GetFromJsonAsync<JsonDocument>("/openapi/v1.json", Cancellation);
        var words = document!.RootElement.GetProperty("paths").GetProperty("/api/words").GetProperty("get");
        var number = words.GetProperty("parameters").EnumerateArray()
            .Single(parameter => parameter.GetProperty("name").GetString() == "number");

        words.GetProperty("summary").GetString().Should().Be("Converts a decimal number to words in the selected language.");
        number.GetProperty("description").GetString().Should().Contain("invariant-culture notation");
    }

    [Fact]
    public async Task TheLanguagesAreTheOnesHsCoreWrites()
    {
        var languages = await _client.GetFromJsonAsync<LanguageOption[]>("/api/languages", Cancellation);

        languages!.Select(language => language.Culture).Should().Equal("en-AU", "nl-NL", "ja-JP");
    }

    [Fact]
    public async Task TheChequeCurrenciesIncludeTheYenWithNoMinorUnit()
    {
        var currencies = await _client.GetFromJsonAsync<CurrencyOption[]>("/api/currencies", Cancellation);

        currencies.Should().ContainSingle(currency => currency.Code == "JPY")
            .Which.Should().BeEquivalentTo(new { DecimalDigits = 0, MinorUnit = 1m });
    }

    [Theory]
    [InlineData("21", "en-AU", "twenty-one")]
    [InlineData("21", "nl-NL", "eenentwintig")]
    [InlineData("123456789", "ja-JP", "一億二千三百四十五万六千七百八十九")]
    [InlineData("-12.5", "en-AU", "minus twelve point five")]
    public async Task ANumberIsWrittenInTheCulturesLanguage(string number, string culture, string words)
    {
        var result = await _client.GetFromJsonAsync<NumberInWords>($"/api/words?number={number}&culture={culture}", Cancellation);

        result.Should().Be(new NumberInWords(number, words));
    }

    [Fact]
    public async Task WithoutACultureNumbersAreInAustralianEnglish()
    {
        var result = await _client.GetFromJsonAsync<NumberInWords>("/api/words?number=105", Cancellation);

        result!.Words.Should().Be("one hundred and five");
    }

    [Fact]
    public async Task AnAmountIsRoundedToItsCurrencyThenWritten()
    {
        var result = await _client.GetFromJsonAsync<AmountInWords>("/api/amount?value=1234.56&currency=JPY&culture=ja-JP", Cancellation);

        result.Should().Be(new AmountInWords("1235", "JPY", "¥1,235", "千二百三十五円"));
    }

    [Fact]
    public async Task ACurrencyCodeIsCaseInsensitive()
    {
        var result = await _client.GetFromJsonAsync<AmountInWords>("/api/amount?value=1.5&currency=eur&culture=en-AU", Cancellation);

        result!.Words.Should().Be("one euro and fifty cents");
    }

    [Fact]
    public async Task DividingLosesNothing()
    {
        var result = await _client.GetFromJsonAsync<Division>("/api/divide?value=0.05&currency=EUR&shares=2", Cancellation);

        result!.Shares.Should().Equal("0.02", "0.03");
    }

    [Theory]
    [InlineData("/api/words?number=lots", "number")]
    [InlineData("/api/words?number=1&culture=xx-NOPE", "culture")]
    [InlineData("/api/amount?value=1&currency=ZZZ", "currency")]
    [InlineData("/api/amount?currency=EUR", "value")]
    [InlineData("/api/divide?value=1&currency=EUR&shares=1", "shares")]
    [InlineData("/api/divide?value=1&currency=EUR", "shares")]
    public async Task InvalidInputIsAValidationProblemNamingTheParameter(string url, string parameter)
    {
        var response = await _client.GetAsync(url, Cancellation);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(Cancellation);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        problem!.Errors.Keys.Should().Equal(parameter);
    }

    [Fact]
    public async Task AnUnknownApiRouteIsNotFound()
    {
        var response = await _client.GetAsync("/api/nothing-here", Cancellation);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

}
