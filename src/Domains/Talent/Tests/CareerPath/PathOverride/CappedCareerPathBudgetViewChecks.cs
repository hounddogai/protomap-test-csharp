namespace Acme.Workforce.Talent.Tests.CareerPath.PathOverride;

using Acme.Workforce.Talent.CareerPath.PathOverride;

/// <summary>Checks of path override code, run by the project's runner.</summary>
public static class CappedCareerPathBudgetViewChecks
{
    public static void CappedCareerPathBudgetViewShowsTheStatusInCapitals()
    {
        var item = CappedCareerPathBudget.NewCappedCareerPathBudget("code-540", "label-97");
        var view = CappedCareerPathBudgetView.ToCappedCareerPathBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedCareerPathBudgetView.ToCappedCareerPathBudgetViews(new List<CappedCareerPathBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedCareerPathBudgetViewShowsTheStatusInCapitals();
    }
}
