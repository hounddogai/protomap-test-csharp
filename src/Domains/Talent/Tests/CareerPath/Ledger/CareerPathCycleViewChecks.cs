namespace Acme.Workforce.Talent.Tests.CareerPath.Ledger;

using Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class CareerPathCycleViewChecks
{
    public static void CareerPathCycleViewShowsTheStatusInCapitals()
    {
        var item = CareerPathCycle.NewCareerPathCycle("code-781", "name-312");
        var view = CareerPathCycleView.ToCareerPathCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CareerPathCycleView.ToCareerPathCycleViews(new List<CareerPathCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CareerPathCycleViewShowsTheStatusInCapitals();
    }
}
