namespace Acme.Workforce.Talent.Tests.CareerPath.Allocation;

using Acme.Workforce.Talent.CareerPath.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class CareerPathAdjustmentViewChecks
{
    public static void CareerPathAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = CareerPathAdjustment.NewCareerPathAdjustment("code-447", "title-343");
        var view = CareerPathAdjustmentView.ToCareerPathAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CareerPathAdjustmentView.ToCareerPathAdjustmentViews(new List<CareerPathAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CareerPathAdjustmentViewShowsTheStatusInCapitals();
    }
}
