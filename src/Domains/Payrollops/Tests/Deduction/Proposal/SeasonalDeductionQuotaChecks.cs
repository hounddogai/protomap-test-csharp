namespace Acme.Workforce.Payrollops.Tests.Deduction.Proposal;

using Acme.Workforce.Payrollops.Deduction.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SeasonalDeductionQuotaChecks
{
    public static void SeasonalDeductionQuotaStartsInPlanned()
    {
        var item = SeasonalDeductionQuota.NewSeasonalDeductionQuota("code-242", "label-335");
        Check.Equal(item.Status, SeasonalDeductionQuotaStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalDeductionQuotaMovesToCompleted()
    {
        var item = SeasonalDeductionQuota.NewSeasonalDeductionQuota("code-242", "label-335");
        try
        {
            item.MoveTo(SeasonalDeductionQuotaStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalDeductionQuotaStatus.Completed, "status");
    }

    public static void SeasonalDeductionQuotaRejectsMovesFromCancelled()
    {
        var item = SeasonalDeductionQuota.NewSeasonalDeductionQuota("code-242", "label-335");
        item.Status = SeasonalDeductionQuotaStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SeasonalDeductionQuotaStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SeasonalDeductionQuotaStartsInPlanned();
        SeasonalDeductionQuotaMovesToCompleted();
        SeasonalDeductionQuotaRejectsMovesFromCancelled();
    }
}
