namespace Acme.Workforce.Assets.Tests.Laptop.LaptopAdjustment;

using Acme.Workforce.Assets.Laptop.LaptopAdjustment;

/// <summary>Checks of laptop adjustment code, run by the project's runner.</summary>
public static class LaptopClaimViewChecks
{
    public static void LaptopClaimViewShowsTheStatusInCapitals()
    {
        var item = LaptopClaim.NewLaptopClaim("code-98", "name-931");
        var view = LaptopClaimView.ToLaptopClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LaptopClaimView.ToLaptopClaimViews(new List<LaptopClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LaptopClaimViewShowsTheStatusInCapitals();
    }
}
