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

        api.MapGet("/languages", () => Catalogue.Languages);
        api.MapGet("/currencies", () => Catalogue.ChequeCurrencies);
        api.MapGet("/words", Words);
        api.MapGet("/amount", Amount);
        api.MapGet("/divide", Divide);

        return app;
    }

    /// <summary><c>GET /api/words?number=21&amp;culture=nl-NL</c>: "eenentwintig".</summary>
    internal static Results<Ok<NumberInWords>, ValidationProblem> Words(string? number, string? culture)
    {
        var (parsed, language) = (Input.Number(number), Input.Culture(culture));
        var errors = Input.Errors((nameof(number), parsed.Error), (nameof(culture), language.Error));

        return errors.Count > 0
            ? TypedResults.ValidationProblem(errors)
            : TypedResults.Ok(new NumberInWords(Invariant(parsed.Value), parsed.Value.ToWords(language.Value!).Value));
    }

    /// <summary><c>GET /api/amount?value=1234.56&amp;currency=JPY&amp;culture=ja-JP</c>: 1235 JPY, "¥1,235", "千二百三十五円".</summary>
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

    /// <summary><c>GET /api/divide?value=0.05&amp;currency=EUR&amp;shares=2</c>: 0.02 and 0.03, nothing lost.</summary>
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
