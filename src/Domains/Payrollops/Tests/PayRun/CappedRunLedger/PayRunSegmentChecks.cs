namespace Acme.Workforce.Payrollops.Tests.PayRun.CappedRunLedger;

using Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>Checks of capped run ledger code, run by the project's runner.</summary>
public static class PayRunSegmentChecks
{
    public static void PayRunSegmentStartsInIdle()
    {
        var item = PayRunSegment.NewPayRunSegment("code-355", "label-230");
        Check.Equal(item.Status, PayRunSegmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunSegmentMovesToReady()
    {
        var item = PayRunSegment.NewPayRunSegment("code-355", "label-230");
        try
        {
            item.MoveTo(PayRunSegmentStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunSegmentStatus.Ready, "status");
    }

    public static void PayRunSegmentRejectsMovesFromStopped()
    {
        var item = PayRunSegment.NewPayRunSegment("code-355", "label-230");
        item.Status = PayRunSegmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(PayRunSegmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PayRunSegmentStartsInIdle();
        PayRunSegmentMovesToReady();
        PayRunSegmentRejectsMovesFromStopped();
    }
}
