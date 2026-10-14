namespace Acme.Workforce.Payrollops.Tests.PayRun.Adjustment;

using Acme.Workforce.Payrollops.PayRun.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PayRunAdjustmentChecks
{
    public static void PayRunAdjustmentStartsInRequested()
    {
        var item = PayRunAdjustment.NewPayRunAdjustment("code-746", "name-841");
        Check.Equal(item.Status, PayRunAdjustmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunAdjustmentMovesToGranted()
    {
        var item = PayRunAdjustment.NewPayRunAdjustment("code-746", "name-841");
        try
        {
            item.MoveTo(PayRunAdjustmentStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunAdjustmentStatus.Granted, "status");
    }

    public static void PayRunAdjustmentRejectsMovesFromExpired()
    {
        var item = PayRunAdjustment.NewPayRunAdjustment("code-746", "name-841");
        item.Status = PayRunAdjustmentStatus.Expired;
        Check.Fails(() => item.MoveTo(PayRunAdjustmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PayRunAdjustmentStartsInRequested();
        PayRunAdjustmentMovesToGranted();
        PayRunAdjustmentRejectsMovesFromExpired();
    }
}
