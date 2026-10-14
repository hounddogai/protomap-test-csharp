namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Forecast;

using Acme.Workforce.Talent.SuccessionPlan.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class AdaptiveSuccessionPlanPolicyChecks
{
    public static void AdaptiveSuccessionPlanPolicyStartsInOpen()
    {
        var item = AdaptiveSuccessionPlanPolicy.NewAdaptiveSuccessionPlanPolicy("code-832", "name-493");
        Check.Equal(item.Status, AdaptiveSuccessionPlanPolicyStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveSuccessionPlanPolicyMovesToResolved()
    {
        var item = AdaptiveSuccessionPlanPolicy.NewAdaptiveSuccessionPlanPolicy("code-832", "name-493");
        try
        {
            item.MoveTo(AdaptiveSuccessionPlanPolicyStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveSuccessionPlanPolicyStatus.Resolved, "status");
    }

    public static void AdaptiveSuccessionPlanPolicyRejectsMovesFromArchived()
    {
        var item = AdaptiveSuccessionPlanPolicy.NewAdaptiveSuccessionPlanPolicy("code-832", "name-493");
        item.Status = AdaptiveSuccessionPlanPolicyStatus.Archived;
        Check.Fails(() => item.MoveTo(AdaptiveSuccessionPlanPolicyStatus.Open), "move");
    }

    public static void RunAll()
    {
        AdaptiveSuccessionPlanPolicyStartsInOpen();
        AdaptiveSuccessionPlanPolicyMovesToResolved();
        AdaptiveSuccessionPlanPolicyRejectsMovesFromArchived();
    }
}
