namespace Acme.Workforce.Payrollops.Tests.Deduction.Segment;

using Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class StrictDeductionQuotaChecks
{
    public static void StrictDeductionQuotaStartsInOpen()
    {
        var item = StrictDeductionQuota.NewStrictDeductionQuota("code-365", "label-753");
        Check.Equal(item.Status, StrictDeductionQuotaStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictDeductionQuotaMovesToInReview()
    {
        var item = StrictDeductionQuota.NewStrictDeductionQuota("code-365", "label-753");
        try
        {
            item.MoveTo(StrictDeductionQuotaStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictDeductionQuotaStatus.InReview, "status");
    }

    public static void StrictDeductionQuotaRejectsMovesFromArchived()
    {
        var item = StrictDeductionQuota.NewStrictDeductionQuota("code-365", "label-753");
        item.Status = StrictDeductionQuotaStatus.Archived;
        Check.Fails(() => item.MoveTo(StrictDeductionQuotaStatus.Open), "move");
    }

    public static void RunAll()
    {
        StrictDeductionQuotaStartsInOpen();
        StrictDeductionQuotaMovesToInReview();
        StrictDeductionQuotaRejectsMovesFromArchived();
    }
}
