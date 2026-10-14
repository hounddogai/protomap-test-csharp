namespace Acme.Workforce.Payrollops.Tests.PayRun.Target;

using Acme.Workforce.Payrollops.PayRun.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class RegionalPayRunGroupChecks
{
    public static void RegionalPayRunGroupStartsInIdle()
    {
        var item = RegionalPayRunGroup.NewRegionalPayRunGroup("code-923", "name-878");
        Check.Equal(item.Status, RegionalPayRunGroupStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalPayRunGroupMovesToStopped()
    {
        var item = RegionalPayRunGroup.NewRegionalPayRunGroup("code-923", "name-878");
        try
        {
            item.MoveTo(RegionalPayRunGroupStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalPayRunGroupStatus.Stopped, "status");
    }

    public static void RegionalPayRunGroupRejectsMovesFromStopped()
    {
        var item = RegionalPayRunGroup.NewRegionalPayRunGroup("code-923", "name-878");
        item.Status = RegionalPayRunGroupStatus.Stopped;
        Check.Fails(() => item.MoveTo(RegionalPayRunGroupStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RegionalPayRunGroupStartsInIdle();
        RegionalPayRunGroupMovesToStopped();
        RegionalPayRunGroupRejectsMovesFromStopped();
    }
}
