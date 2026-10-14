namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Variance;

using Acme.Workforce.Talent.SuccessionPlan.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class SuccessionPlanVarianceChecks
{
    public static void SuccessionPlanVarianceStartsInIdle()
    {
        var item = SuccessionPlanVariance.NewSuccessionPlanVariance("code-346", "name-173");
        Check.Equal(item.Status, SuccessionPlanVarianceStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanVarianceMovesToDraining()
    {
        var item = SuccessionPlanVariance.NewSuccessionPlanVariance("code-346", "name-173");
        try
        {
            item.MoveTo(SuccessionPlanVarianceStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanVarianceStatus.Draining, "status");
    }

    public static void SuccessionPlanVarianceRejectsMovesFromStopped()
    {
        var item = SuccessionPlanVariance.NewSuccessionPlanVariance("code-346", "name-173");
        item.Status = SuccessionPlanVarianceStatus.Stopped;
        Check.Fails(() => item.MoveTo(SuccessionPlanVarianceStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanVarianceStartsInIdle();
        SuccessionPlanVarianceMovesToDraining();
        SuccessionPlanVarianceRejectsMovesFromStopped();
    }
}
