namespace Acme.Workforce.Engagement.Tests.TownHall.Ledger;

using Acme.Workforce.Engagement.TownHall.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RegionalTownHallMappingChecks
{
    public static void RegionalTownHallMappingStartsInNew()
    {
        var item = RegionalTownHallMapping.NewRegionalTownHallMapping("code-251", "name-825");
        Check.Equal(item.Status, RegionalTownHallMappingStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalTownHallMappingMovesToDone()
    {
        var item = RegionalTownHallMapping.NewRegionalTownHallMapping("code-251", "name-825");
        try
        {
            item.MoveTo(RegionalTownHallMappingStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalTownHallMappingStatus.Done, "status");
    }

    public static void RegionalTownHallMappingRejectsMovesFromFailed()
    {
        var item = RegionalTownHallMapping.NewRegionalTownHallMapping("code-251", "name-825");
        item.Status = RegionalTownHallMappingStatus.Failed;
        Check.Fails(() => item.MoveTo(RegionalTownHallMappingStatus.New), "move");
    }

    public static void RunAll()
    {
        RegionalTownHallMappingStartsInNew();
        RegionalTownHallMappingMovesToDone();
        RegionalTownHallMappingRejectsMovesFromFailed();
    }
}
