namespace Acme.Workforce.Talent.Tests.CareerPath.Draft;

using Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class CareerPathBudgetViewChecks
{
    public static void CareerPathBudgetViewShowsTheStatusInCapitals()
    {
        var item = CareerPathBudget.NewCareerPathBudget("code-906", "name-726");
        var view = CareerPathBudgetView.ToCareerPathBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CareerPathBudgetView.ToCareerPathBudgetViews(new List<CareerPathBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CareerPathBudgetViewShowsTheStatusInCapitals();
    }
}
