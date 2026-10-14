namespace Acme.Workforce.Orgdesign.Tests.Headcount.Budget;

using Acme.Workforce.Orgdesign.Headcount.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class HeadcountRollupViewChecks
{
    public static void HeadcountRollupViewShowsTheStatusInCapitals()
    {
        var item = HeadcountRollup.NewHeadcountRollup("code-970", "label-400");
        var view = HeadcountRollupView.ToHeadcountRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HeadcountRollupView.ToHeadcountRollupViews(new List<HeadcountRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HeadcountRollupViewShowsTheStatusInCapitals();
    }
}
