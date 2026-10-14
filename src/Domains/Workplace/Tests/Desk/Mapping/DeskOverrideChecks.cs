namespace Acme.Workforce.Workplace.Tests.Desk.Mapping;

using Acme.Workforce.Workplace.Desk.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DeskOverrideChecks
{
    public static void DeskOverrideStartsInOpen()
    {
        var item = DeskOverride.NewDeskOverride("code-494", "label-224");
        Check.Equal(item.Status, DeskOverrideStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskOverrideMovesToResolved()
    {
        var item = DeskOverride.NewDeskOverride("code-494", "label-224");
        try
        {
            item.MoveTo(DeskOverrideStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskOverrideStatus.Resolved, "status");
    }

    public static void DeskOverrideRejectsMovesFromArchived()
    {
        var item = DeskOverride.NewDeskOverride("code-494", "label-224");
        item.Status = DeskOverrideStatus.Archived;
        Check.Fails(() => item.MoveTo(DeskOverrideStatus.Open), "move");
    }

    public static void RunAll()
    {
        DeskOverrideStartsInOpen();
        DeskOverrideMovesToResolved();
        DeskOverrideRejectsMovesFromArchived();
    }
}
