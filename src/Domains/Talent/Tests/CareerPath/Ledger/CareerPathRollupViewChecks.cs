namespace Acme.Workforce.Talent.Tests.CareerPath.Ledger;

using Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class CareerPathRollupViewChecks
{
    public static void CareerPathRollupViewShowsTheStatusInCapitals()
    {
        var item = CareerPathRollup.NewCareerPathRollup("code-350", "label-459");
        var view = CareerPathRollupView.ToCareerPathRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CareerPathRollupView.ToCareerPathRollupViews(new List<CareerPathRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CareerPathRollupViewShowsTheStatusInCapitals();
    }
}
