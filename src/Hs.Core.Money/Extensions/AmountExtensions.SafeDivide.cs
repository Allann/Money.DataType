namespace Hs.Core.Money.Extensions;

/// <summary>Extensions for <see cref="Amount"/>.</summary>
public static class AmountExtensions
{
    extension(Amount money)
    {
        /// <summary>Divides <paramref name="money"/> into equal shares without losing any of it, e.g. 0.05 EUR in two is 0.02 and 0.03.</summary>
        /// <remarks>Each share is rounded to the currency's minor unit with banker's rounding; the last share takes the remainder.</remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="shares"/> is less than 2.</exception>
        public IEnumerable<Amount> SafeDivide(int shares) => money.SafeDivide(shares, MidpointRounding.ToEven);

        /// <summary>Divides <paramref name="money"/> into equal shares without losing any of it.</summary>
        /// <remarks>Each share is rounded to the currency's minor unit with <paramref name="rounding"/>; the last share takes the remainder.</remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="shares"/> is less than 2.</exception>
        public IEnumerable<Amount> SafeDivide(int shares, MidpointRounding rounding)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(shares, 2);
            return Allocate(money, Enumerable.Repeat(1, shares).ToArray(), rounding);
        }

        /// <summary>Divides <paramref name="money"/> by <paramref name="ratios"/> without losing any of it, e.g. 1 EUR by 2:3:3 is 0.25, 0.38 and 0.37.</summary>
        /// <remarks>Each share is rounded to the currency's minor unit with banker's rounding; the last share takes the remainder.</remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="ratios"/> is empty or contains a ratio less than 1.</exception>
        public IEnumerable<Amount> SafeDivide(int[] ratios) => money.SafeDivide(ratios, MidpointRounding.ToEven);

        /// <summary>Divides <paramref name="money"/> by <paramref name="ratios"/> without losing any of it.</summary>
        /// <remarks>Each share is rounded to the currency's minor unit with <paramref name="rounding"/>; the last share takes the remainder.</remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="ratios"/> is empty or contains a ratio less than 1.</exception>
        public IEnumerable<Amount> SafeDivide(int[] ratios, MidpointRounding rounding)
        {
            ArgumentNullException.ThrowIfNull(ratios);
            if (ratios.Length == 0 || ratios.Any(ratio => ratio < 1))
            {
                throw new ArgumentOutOfRangeException(nameof(ratios), $"There must be at least one ratio, and all {nameof(ratios)} must be greater than or equal to 1.");
            }

            return Allocate(money, ratios, rounding);
        }
    }

    private static Amount[] Allocate(Amount money, int[] ratios, MidpointRounding rounding)
    {
        var total = ratios.Sum();
        var shares = ratios[..^1].Select(ratio => money.Currency.Round(money.Value * ratio / total, rounding)).ToArray();
        var remainder = money.Value - shares.Sum();

        return [.. shares.Select(share => new Amount(share, money.Currency)), new Amount(remainder, money.Currency)];
    }
}
