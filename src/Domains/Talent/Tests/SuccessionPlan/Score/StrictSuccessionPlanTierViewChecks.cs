namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Score;

using Acme.Workforce.Talent.SuccessionPlan.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictSuccessionPlanTierViewChecks
{
    public static void StrictSuccessionPlanTierViewShowsTheStatusInCapitals()
    {
        var item = StrictSuccessionPlanTier.NewStrictSuccessionPlanTier("code-235", "name-157");
        var view = StrictSuccessionPlanTierView.ToStrictSuccessionPlanTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictSuccessionPlanTierView.ToStrictSuccessionPlanTierViews(new List<StrictSuccessionPlanTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictSuccessionPlanTierViewShowsTheStatusInCapitals();
    }
}
