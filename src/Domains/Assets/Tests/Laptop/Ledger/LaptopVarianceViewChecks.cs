namespace Acme.Workforce.Assets.Tests.Laptop.Ledger;

using Acme.Workforce.Assets.Laptop.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LaptopVarianceViewChecks
{
    public static void LaptopVarianceViewShowsTheStatusInCapitals()
    {
        var item = LaptopVariance.NewLaptopVariance("code-925", "title-582");
        var view = LaptopVarianceView.ToLaptopVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LaptopVarianceView.ToLaptopVarianceViews(new List<LaptopVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LaptopVarianceViewShowsTheStatusInCapitals();
    }
}
