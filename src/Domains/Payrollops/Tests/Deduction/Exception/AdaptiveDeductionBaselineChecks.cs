namespace Acme.Workforce.Payrollops.Tests.Deduction.Exception;

using Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class AdaptiveDeductionBaselineChecks
{
    public static void AdaptiveDeductionBaselineStartsInOpen()
    {
        var item = AdaptiveDeductionBaseline.NewAdaptiveDeductionBaseline("code-969", "title-124");
        Check.Equal(item.Status, AdaptiveDeductionBaselineStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveDeductionBaselineMovesToInReview()
    {
        var item = AdaptiveDeductionBaseline.NewAdaptiveDeductionBaseline("code-969", "title-124");
        try
        {
            item.MoveTo(AdaptiveDeductionBaselineStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveDeductionBaselineStatus.InReview, "status");
    }

    public static void AdaptiveDeductionBaselineRejectsMovesFromArchived()
    {
        var item = AdaptiveDeductionBaseline.NewAdaptiveDeductionBaseline("code-969", "title-124");
        item.Status = AdaptiveDeductionBaselineStatus.Archived;
        Check.Fails(() => item.MoveTo(AdaptiveDeductionBaselineStatus.Open), "move");
    }

    public static void RunAll()
    {
        AdaptiveDeductionBaselineStartsInOpen();
        AdaptiveDeductionBaselineMovesToInReview();
        AdaptiveDeductionBaselineRejectsMovesFromArchived();
    }
}
