namespace Acme.Workforce.Payrollops.Tests.Deduction.Agreement;

using Acme.Workforce.Payrollops.Deduction.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class DeductionAdjustmentChecks
{
    public static void DeductionAdjustmentStartsInRequested()
    {
        var item = DeductionAdjustment.NewDeductionAdjustment("code-251", "title-745");
        Check.Equal(item.Status, DeductionAdjustmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionAdjustmentMovesToGranted()
    {
        var item = DeductionAdjustment.NewDeductionAdjustment("code-251", "title-745");
        try
        {
            item.MoveTo(DeductionAdjustmentStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionAdjustmentStatus.Granted, "status");
    }

    public static void DeductionAdjustmentRejectsMovesFromExpired()
    {
        var item = DeductionAdjustment.NewDeductionAdjustment("code-251", "title-745");
        item.Status = DeductionAdjustmentStatus.Expired;
        Check.Fails(() => item.MoveTo(DeductionAdjustmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        DeductionAdjustmentStartsInRequested();
        DeductionAdjustmentMovesToGranted();
        DeductionAdjustmentRejectsMovesFromExpired();
    }
}
