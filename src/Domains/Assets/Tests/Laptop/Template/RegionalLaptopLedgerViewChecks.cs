namespace Acme.Workforce.Assets.Tests.Laptop.Template;

using Acme.Workforce.Assets.Laptop.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class RegionalLaptopLedgerViewChecks
{
    public static void RegionalLaptopLedgerViewShowsTheStatusInCapitals()
    {
        var item = RegionalLaptopLedger.NewRegionalLaptopLedger("code-593", "title-669");
        var view = RegionalLaptopLedgerView.ToRegionalLaptopLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalLaptopLedgerView.ToRegionalLaptopLedgerViews(new List<RegionalLaptopLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalLaptopLedgerViewShowsTheStatusInCapitals();
    }
}
