namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Window;

using Acme.Workforce.Talent.SuccessionPlan.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class SuccessionPlanThresholdChecks
{
    public static void SuccessionPlanThresholdStartsInProposed()
    {
        var item = SuccessionPlanThreshold.NewSuccessionPlanThreshold("code-729", "title-269");
        Check.Equal(item.Status, SuccessionPlanThresholdStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanThresholdMovesToRejected()
    {
        var item = SuccessionPlanThreshold.NewSuccessionPlanThreshold("code-729", "title-269");
        try
        {
            item.MoveTo(SuccessionPlanThresholdStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanThresholdStatus.Rejected, "status");
    }

    public static void SuccessionPlanThresholdRejectsMovesFromWithdrawn()
    {
        var item = SuccessionPlanThreshold.NewSuccessionPlanThreshold("code-729", "title-269");
        item.Status = SuccessionPlanThresholdStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SuccessionPlanThresholdStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanThresholdStartsInProposed();
        SuccessionPlanThresholdMovesToRejected();
        SuccessionPlanThresholdRejectsMovesFromWithdrawn();
    }
}
