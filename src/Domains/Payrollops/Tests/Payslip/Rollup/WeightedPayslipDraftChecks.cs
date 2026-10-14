namespace Acme.Workforce.Payrollops.Tests.Payslip.Rollup;

using Acme.Workforce.Payrollops.Payslip.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class WeightedPayslipDraftChecks
{
    public static void WeightedPayslipDraftStartsInRequested()
    {
        var item = WeightedPayslipDraft.NewWeightedPayslipDraft("code-738", "label-979");
        Check.Equal(item.Status, WeightedPayslipDraftStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedPayslipDraftMovesToRevoked()
    {
        var item = WeightedPayslipDraft.NewWeightedPayslipDraft("code-738", "label-979");
        try
        {
            item.MoveTo(WeightedPayslipDraftStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedPayslipDraftStatus.Revoked, "status");
    }

    public static void WeightedPayslipDraftRejectsMovesFromExpired()
    {
        var item = WeightedPayslipDraft.NewWeightedPayslipDraft("code-738", "label-979");
        item.Status = WeightedPayslipDraftStatus.Expired;
        Check.Fails(() => item.MoveTo(WeightedPayslipDraftStatus.Requested), "move");
    }

    public static void RunAll()
    {
        WeightedPayslipDraftStartsInRequested();
        WeightedPayslipDraftMovesToRevoked();
        WeightedPayslipDraftRejectsMovesFromExpired();
    }
}
