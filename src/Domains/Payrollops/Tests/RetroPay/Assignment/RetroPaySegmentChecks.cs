namespace Acme.Workforce.Payrollops.Tests.RetroPay.Assignment;

using Acme.Workforce.Payrollops.RetroPay.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class RetroPaySegmentChecks
{
    public static void RetroPaySegmentStartsInIdle()
    {
        var item = RetroPaySegment.NewRetroPaySegment("code-425", "title-253");
        Check.Equal(item.Status, RetroPaySegmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPaySegmentMovesToWarming()
    {
        var item = RetroPaySegment.NewRetroPaySegment("code-425", "title-253");
        try
        {
            item.MoveTo(RetroPaySegmentStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPaySegmentStatus.Warming, "status");
    }

    public static void RetroPaySegmentRejectsMovesFromStopped()
    {
        var item = RetroPaySegment.NewRetroPaySegment("code-425", "title-253");
        item.Status = RetroPaySegmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(RetroPaySegmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RetroPaySegmentStartsInIdle();
        RetroPaySegmentMovesToWarming();
        RetroPaySegmentRejectsMovesFromStopped();
    }
}
