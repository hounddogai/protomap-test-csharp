namespace Acme.Workforce.Contractors.Tests.Rate.Window;

using Acme.Workforce.Contractors.Rate.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TieredRateWindowChecks
{
    public static void TieredRateWindowStartsInDraft()
    {
        var item = TieredRateWindow.NewTieredRateWindow("code-871", "name-456");
        Check.Equal(item.Status, TieredRateWindowStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredRateWindowMovesToActive()
    {
        var item = TieredRateWindow.NewTieredRateWindow("code-871", "name-456");
        try
        {
            item.MoveTo(TieredRateWindowStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredRateWindowStatus.Active, "status");
    }

    public static void TieredRateWindowRejectsMovesFromClosed()
    {
        var item = TieredRateWindow.NewTieredRateWindow("code-871", "name-456");
        item.Status = TieredRateWindowStatus.Closed;
        Check.Fails(() => item.MoveTo(TieredRateWindowStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TieredRateWindowStartsInDraft();
        TieredRateWindowMovesToActive();
        TieredRateWindowRejectsMovesFromClosed();
    }
}
