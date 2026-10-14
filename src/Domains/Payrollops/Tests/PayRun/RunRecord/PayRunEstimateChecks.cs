namespace Acme.Workforce.Payrollops.Tests.PayRun.RunRecord;

using Acme.Workforce.Payrollops.PayRun.RunRecord;

/// <summary>Checks of run record code, run by the project's runner.</summary>
public static class PayRunEstimateChecks
{
    public static void PayRunEstimateStartsInProposed()
    {
        var item = PayRunEstimate.NewPayRunEstimate("code-930", "name-550");
        Check.Equal(item.Status, PayRunEstimateStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunEstimateMovesToWithdrawn()
    {
        var item = PayRunEstimate.NewPayRunEstimate("code-930", "name-550");
        try
        {
            item.MoveTo(PayRunEstimateStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunEstimateStatus.Withdrawn, "status");
    }

    public static void PayRunEstimateRejectsMovesFromWithdrawn()
    {
        var item = PayRunEstimate.NewPayRunEstimate("code-930", "name-550");
        item.Status = PayRunEstimateStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PayRunEstimateStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PayRunEstimateStartsInProposed();
        PayRunEstimateMovesToWithdrawn();
        PayRunEstimateRejectsMovesFromWithdrawn();
    }
}
