namespace Acme.Workforce.Workplace.Tests.Desk.Mapping;

using Acme.Workforce.Workplace.Desk.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DeskMappingChecks
{
    public static void DeskMappingStartsInIdle()
    {
        var item = DeskMapping.NewDeskMapping("code-662", "title-988");
        Check.Equal(item.Status, DeskMappingStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskMappingMovesToStopped()
    {
        var item = DeskMapping.NewDeskMapping("code-662", "title-988");
        try
        {
            item.MoveTo(DeskMappingStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskMappingStatus.Stopped, "status");
    }

    public static void DeskMappingRejectsMovesFromStopped()
    {
        var item = DeskMapping.NewDeskMapping("code-662", "title-988");
        item.Status = DeskMappingStatus.Stopped;
        Check.Fails(() => item.MoveTo(DeskMappingStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DeskMappingStartsInIdle();
        DeskMappingMovesToStopped();
        DeskMappingRejectsMovesFromStopped();
    }
}
