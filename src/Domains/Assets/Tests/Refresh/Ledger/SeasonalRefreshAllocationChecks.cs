namespace Acme.Workforce.Assets.Tests.Refresh.Ledger;

using Acme.Workforce.Assets.Refresh.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SeasonalRefreshAllocationChecks
{
    public static void SeasonalRefreshAllocationStartsInRequested()
    {
        var item = SeasonalRefreshAllocation.NewSeasonalRefreshAllocation("code-279", "name-531");
        Check.Equal(item.Status, SeasonalRefreshAllocationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalRefreshAllocationMovesToGranted()
    {
        var item = SeasonalRefreshAllocation.NewSeasonalRefreshAllocation("code-279", "name-531");
        try
        {
            item.MoveTo(SeasonalRefreshAllocationStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalRefreshAllocationStatus.Granted, "status");
    }

    public static void SeasonalRefreshAllocationRejectsMovesFromExpired()
    {
        var item = SeasonalRefreshAllocation.NewSeasonalRefreshAllocation("code-279", "name-531");
        item.Status = SeasonalRefreshAllocationStatus.Expired;
        Check.Fails(() => item.MoveTo(SeasonalRefreshAllocationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SeasonalRefreshAllocationStartsInRequested();
        SeasonalRefreshAllocationMovesToGranted();
        SeasonalRefreshAllocationRejectsMovesFromExpired();
    }
}
