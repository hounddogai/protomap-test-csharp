namespace Acme.Workforce.Contractors.Tests.Rate.Slot;

using Acme.Workforce.Contractors.Rate.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CappedRatePolicyChecks
{
    public static void CappedRatePolicyStartsInPlanned()
    {
        var item = CappedRatePolicy.NewCappedRatePolicy("code-791", "title-783");
        Check.Equal(item.Status, CappedRatePolicyStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedRatePolicyMovesToScheduled()
    {
        var item = CappedRatePolicy.NewCappedRatePolicy("code-791", "title-783");
        try
        {
            item.MoveTo(CappedRatePolicyStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedRatePolicyStatus.Scheduled, "status");
    }

    public static void CappedRatePolicyRejectsMovesFromCancelled()
    {
        var item = CappedRatePolicy.NewCappedRatePolicy("code-791", "title-783");
        item.Status = CappedRatePolicyStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CappedRatePolicyStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CappedRatePolicyStartsInPlanned();
        CappedRatePolicyMovesToScheduled();
        CappedRatePolicyRejectsMovesFromCancelled();
    }
}
