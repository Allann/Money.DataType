using Hs.Core.Money.Tests.Helpers;

// Many specs create amounts in "the current culture's currency". Pin the culture they were written against,
// so the suite gives the same results on any machine or CI runner; [UseCulture] on a test still overrides it.
[assembly: UseCulture("en-AU")]
