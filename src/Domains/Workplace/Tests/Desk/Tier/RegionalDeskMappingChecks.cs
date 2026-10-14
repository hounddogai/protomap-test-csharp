namespace Acme.Workforce.Workplace.Tests.Desk.Tier;

using Acme.Workforce.Workplace.Desk.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class RegionalDeskMappingChecks
{
    public static void RegionalDeskMappingStartsInOpen()
    {
        var item = RegionalDeskMapping.NewRegionalDeskMapping("code-235", "title-958");
        Check.Equal(item.Status, RegionalDeskMappingStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalDeskMappingMovesToReopened()
    {
        var item = RegionalDeskMapping.NewRegionalDeskMapping("code-235", "title-958");
        try
        {
            item.MoveTo(RegionalDeskMappingStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalDeskMappingStatus.Reopened, "status");
    }

    public static void RegionalDeskMappingRejectsMovesFromArchived()
    {
        var item = RegionalDeskMapping.NewRegionalDeskMapping("code-235", "title-958");
        item.Status = RegionalDeskMappingStatus.Archived;
        Check.Fails(() => item.MoveTo(RegionalDeskMappingStatus.Open), "move");
    }

    public static void RunAll()
    {
        RegionalDeskMappingStartsInOpen();
        RegionalDeskMappingMovesToReopened();
        RegionalDeskMappingRejectsMovesFromArchived();
    }
}
