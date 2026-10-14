namespace Acme.Workforce.Kit.Tests.Money;

using Acme.Workforce.Kit.Money;

/// <summary>Checks of money code, run by the project's runner.</summary>
public static class MoneyChecks
{
    public static void SplitsWithoutLosingACent()
    {
        var total = Money.Of(1001, "usd");
        var shares = total.Split(3);
        Check.Equal(shares.Count, 3, "shares");
        Check.Equal(shares[0].Units, 334, "first share");
        Check.Equal(total.Currency, "USD", "currency");
    }

    public static void RefusesToMixCurrencies()
    {
        var euros = Money.Of(100, "EUR");
        Check.Fails(() => _ = euros.Plus(Money.Of(5, "USD")), "adding dollars to euros");
    }

    public static void RunAll()
    {
        SplitsWithoutLosingACent();
        RefusesToMixCurrencies();
    }
}
