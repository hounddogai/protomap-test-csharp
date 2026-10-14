namespace Acme.Workforce.Orgdesign.Tests.Headcount.HeadcountRecord;

using Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

/// <summary>Checks of headcount record code, run by the project's runner.</summary>
public static class LegacyHeadcountBudgetViewChecks
{
    public static void LegacyHeadcountBudgetViewShowsTheStatusInCapitals()
    {
        var item = LegacyHeadcountBudget.NewLegacyHeadcountBudget("code-63", "title-769");
        var view = LegacyHeadcountBudgetView.ToLegacyHeadcountBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyHeadcountBudgetView.ToLegacyHeadcountBudgetViews(new List<LegacyHeadcountBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyHeadcountBudgetViewShowsTheStatusInCapitals();
    }
}
