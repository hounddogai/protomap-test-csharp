namespace Acme.Workforce.Absence.Tests.Policy.Segment;

using Acme.Workforce.Absence.Policy.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class RollingPolicyEstimateChecks
{
    public static void RollingPolicyEstimateStartsInNew()
    {
        var item = RollingPolicyEstimate.NewRollingPolicyEstimate("code-459", "title-587");
        Check.Equal(item.Status, RollingPolicyEstimateStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingPolicyEstimateMovesToQueued()
    {
        var item = RollingPolicyEstimate.NewRollingPolicyEstimate("code-459", "title-587");
        try
        {
            item.MoveTo(RollingPolicyEstimateStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingPolicyEstimateStatus.Queued, "status");
    }

    public static void RollingPolicyEstimateRejectsMovesFromFailed()
    {
        var item = RollingPolicyEstimate.NewRollingPolicyEstimate("code-459", "title-587");
        item.Status = RollingPolicyEstimateStatus.Failed;
        Check.Fails(() => item.MoveTo(RollingPolicyEstimateStatus.New), "move");
    }

    public static void RunAll()
    {
        RollingPolicyEstimateStartsInNew();
        RollingPolicyEstimateMovesToQueued();
        RollingPolicyEstimateRejectsMovesFromFailed();
    }
}
