using System.Globalization;
using Hs.Core;
using Hs.Core.Money;
using Hs.Core.Money.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Hs.Showcase.Api;

/// <summary>The showcase's HTTP API: a thin layer over Hs.Core and Hs.Core.Money that validates input and shapes output.</summary>
internal static class ShowcaseEndpoints
{
    public static IEndpointRouteBuilder MapShowcase(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");

        api.MapGet("/languages", () => Catalogue.Languages)
            .WithName("GetLanguages")
            .WithSummary("List the supported number-writing languages")
            .WithDescription("Returns the languages available for converting numbers and money amounts to words, including each language's culture code.")
            .Produces<IReadOnlyList<LanguageOption>>(StatusCodes.Status200OK);
        api.MapGet("/currencies", () => Catalogue.ChequeCurrencies)
            .WithName("GetCurrencies")
            .WithSummary("List the currencies supported by the cheque showcase")
            .WithDescription("Returns currency codes, names, symbols, and minor-unit details for currencies that can be written out as cheque amounts.")
            .Produces<IReadOnlyList<CurrencyOption>>(StatusCodes.Status200OK);
        api.MapGet("/words", Words)
            .WithName("WriteNumberInWords")
            .WithSummary("Write a number in words")
            .WithDescription("Converts a decimal number to words in the selected language. The culture defaults to Australian English (en-AU).")
            .Produces<NumberInWords>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);
        api.MapGet("/amount", Amount)
            .WithName("WriteAmountInWords")
            .WithSummary("Write a currency amount in figures and words")
            .WithDescription("Rounds the amount to the currency's minor unit, then returns its invariant numeric value, culture-formatted figures, and words in the selected language. The culture defaults to Australian English (en-AU).")
            .Produces<AmountInWords>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);
        api.MapGet("/divide", Divide)
            .WithName("DivideAmountIntoShares")
            .WithSummary("Divide a currency amount into exact shares")
            .WithDescription("Splits an amount into the requested number of shares. Any remainder is distributed across shares so their sum equals the original amount.")
            .Produces<Division>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        return app;
    }

    /// <summary>Converts a decimal number to words in the selected language.</summary>
    /// <param name="number">The decimal number to convert, written using invariant-culture notation such as 1234.56.</param>
    /// <param name="culture">The language culture code, such as nl-NL or ja-JP. Omit to use Australian English (en-AU).</param>
    /// <returns>The input number in invariant notation and its words in the selected language.</returns>
    internal static Results<Ok<NumberInWords>, ValidationProblem> Words(string? number, string? culture)
    {
        var (parsed, language) = (Input.Number(number), Input.Culture(culture));
        var errors = Input.Errors((nameof(number), parsed.Error), (nameof(culture), language.Error));

        return errors.Count > 0
            ? TypedResults.ValidationProblem(errors)
            : TypedResults.Ok(new NumberInWords(Invariant(parsed.Value), parsed.Value.ToWords(language.Value!).Value));
    }

    /// <summary>Rounds a currency amount to its minor unit and returns it in figures and words.</summary>
    /// <param name="value">The decimal amount to convert, written using invariant-culture notation such as 1234.56.</param>
    /// <param name="currency">The currency's ISO 4217 code, such as EUR or JPY. Codes are case-insensitive.</param>
    /// <param name="culture">The language culture code, such as nl-NL or ja-JP. Omit to use Australian English (en-AU).</param>
    /// <returns>The rounded amount, currency code, culture-formatted figures, and words in the selected language.</returns>
    internal static Results<Ok<AmountInWords>, ValidationProblem> Amount(string? value, string? currency, string? culture)
    {
        var (parsed, unit, language) = (Input.Number(value), Input.Currency(currency), Input.Culture(culture));
        var errors = Input.Errors((nameof(value), parsed.Error), (nameof(currency), unit.Error), (nameof(culture), language.Error));
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var amount = new Amount(parsed.Value, unit.Value);
        return TypedResults.Ok(new AmountInWords(
            Invariant(amount.Value),
            amount.Currency.Code,
            amount.ToString("C", language.Value),
            amount.ToString("W", language.Value)));
    }

    /// <summary>Divides a currency amount into shares whose sum exactly matches the amount.</summary>
    /// <param name="value">The decimal amount to divide, written using invariant-culture notation such as 0.05.</param>
    /// <param name="currency">The currency's ISO 4217 code, such as EUR or JPY. Codes are case-insensitive.</param>
    /// <param name="shares">The number of shares to create; must be between 2 and 100 inclusive.</param>
    /// <returns>The rounded total, currency code, and invariant-formatted share values.</returns>
    internal static Results<Ok<Division>, ValidationProblem> Divide(string? value, string? currency, int? shares)
    {
        var (parsed, unit, count) = (Input.Number(value), Input.Currency(currency), Input.Shares(shares));
        var errors = Input.Errors((nameof(value), parsed.Error), (nameof(currency), unit.Error), (nameof(shares), count.Error));
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var amount = new Amount(parsed.Value, unit.Value);
        return TypedResults.Ok(new Division(
            Invariant(amount.Value),
            amount.Currency.Code,
            [.. amount.SafeDivide(count.Value).Select(share => Invariant(share.Value))]));
    }

    private static string Invariant(decimal value) => value.ToString(CultureInfo.InvariantCulture);
}
