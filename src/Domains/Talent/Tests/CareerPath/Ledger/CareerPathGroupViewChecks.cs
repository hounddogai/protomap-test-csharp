namespace Acme.Workforce.Talent.Tests.CareerPath.Ledger;

using Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class CareerPathGroupViewChecks
{
    public static void CareerPathGroupViewShowsTheStatusInCapitals()
    {
        var item = CareerPathGroup.NewCareerPathGroup("code-234", "label-921");
        var view = CareerPathGroupView.ToCareerPathGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CareerPathGroupView.ToCareerPathGroupViews(new List<CareerPathGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CareerPathGroupViewShowsTheStatusInCapitals();
    }
}
