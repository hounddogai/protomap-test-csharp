namespace Acme.Workforce.Orgdesign.Tests.Headcount.Target;

using Acme.Workforce.Orgdesign.Headcount.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SeasonalHeadcountWindowChecks
{
    public static void SeasonalHeadcountWindowStartsInIdle()
    {
        var item = SeasonalHeadcountWindow.NewSeasonalHeadcountWindow("code-302", "label-369");
        Check.Equal(item.Status, SeasonalHeadcountWindowStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalHeadcountWindowMovesToWarming()
    {
        var item = SeasonalHeadcountWindow.NewSeasonalHeadcountWindow("code-302", "label-369");
        try
        {
            item.MoveTo(SeasonalHeadcountWindowStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalHeadcountWindowStatus.Warming, "status");
    }

    public static void SeasonalHeadcountWindowRejectsMovesFromStopped()
    {
        var item = SeasonalHeadcountWindow.NewSeasonalHeadcountWindow("code-302", "label-369");
        item.Status = SeasonalHeadcountWindowStatus.Stopped;
        Check.Fails(() => item.MoveTo(SeasonalHeadcountWindowStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SeasonalHeadcountWindowStartsInIdle();
        SeasonalHeadcountWindowMovesToWarming();
        SeasonalHeadcountWindowRejectsMovesFromStopped();
    }
}
