namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Target;

using Acme.Workforce.Talent.SuccessionPlan.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LenientSuccessionPlanTargetViewChecks
{
    public static void LenientSuccessionPlanTargetViewShowsTheStatusInCapitals()
    {
        var item = LenientSuccessionPlanTarget.NewLenientSuccessionPlanTarget("code-131", "title-438");
        var view = LenientSuccessionPlanTargetView.ToLenientSuccessionPlanTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientSuccessionPlanTargetView.ToLenientSuccessionPlanTargetViews(new List<LenientSuccessionPlanTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientSuccessionPlanTargetViewShowsTheStatusInCapitals();
    }
}
