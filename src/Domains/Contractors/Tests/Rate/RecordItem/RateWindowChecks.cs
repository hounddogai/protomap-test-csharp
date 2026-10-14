namespace Acme.Workforce.Contractors.Tests.Rate.RecordItem;

using Acme.Workforce.Contractors.Rate.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RateWindowChecks
{
    public static void RateWindowStartsInIdle()
    {
        var item = RateWindow.NewRateWindow("code-895", "title-850");
        Check.Equal(item.Status, RateWindowStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateWindowMovesToWarming()
    {
        var item = RateWindow.NewRateWindow("code-895", "title-850");
        try
        {
            item.MoveTo(RateWindowStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateWindowStatus.Warming, "status");
    }

    public static void RateWindowRejectsMovesFromStopped()
    {
        var item = RateWindow.NewRateWindow("code-895", "title-850");
        item.Status = RateWindowStatus.Stopped;
        Check.Fails(() => item.MoveTo(RateWindowStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RateWindowStartsInIdle();
        RateWindowMovesToWarming();
        RateWindowRejectsMovesFromStopped();
    }
}
