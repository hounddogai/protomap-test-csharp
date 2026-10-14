namespace Acme.Workforce.Compliance.Tests.PolicyAck.Estimate;

using Acme.Workforce.Compliance.PolicyAck.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class SeasonalPolicyAckEstimateChecks
{
    public static void SeasonalPolicyAckEstimateStartsInPlanned()
    {
        var item = SeasonalPolicyAckEstimate.NewSeasonalPolicyAckEstimate("code-926", "label-329");
        Check.Equal(item.Status, SeasonalPolicyAckEstimateStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalPolicyAckEstimateMovesToScheduled()
    {
        var item = SeasonalPolicyAckEstimate.NewSeasonalPolicyAckEstimate("code-926", "label-329");
        try
        {
            item.MoveTo(SeasonalPolicyAckEstimateStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalPolicyAckEstimateStatus.Scheduled, "status");
    }

    public static void SeasonalPolicyAckEstimateRejectsMovesFromCancelled()
    {
        var item = SeasonalPolicyAckEstimate.NewSeasonalPolicyAckEstimate("code-926", "label-329");
        item.Status = SeasonalPolicyAckEstimateStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SeasonalPolicyAckEstimateStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SeasonalPolicyAckEstimateStartsInPlanned();
        SeasonalPolicyAckEstimateMovesToScheduled();
        SeasonalPolicyAckEstimateRejectsMovesFromCancelled();
    }
}
