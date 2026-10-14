namespace Acme.Workforce.Payrollops.Tests.PayRun.Window;

using Acme.Workforce.Payrollops.PayRun.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RegionalPayRunWindowChecks
{
    public static void RegionalPayRunWindowStartsInCreated()
    {
        var item = RegionalPayRunWindow.NewRegionalPayRunWindow("code-496", "label-294");
        Check.Equal(item.Status, RegionalPayRunWindowStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalPayRunWindowMovesToValidated()
    {
        var item = RegionalPayRunWindow.NewRegionalPayRunWindow("code-496", "label-294");
        try
        {
            item.MoveTo(RegionalPayRunWindowStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalPayRunWindowStatus.Validated, "status");
    }

    public static void RegionalPayRunWindowRejectsMovesFromRetired()
    {
        var item = RegionalPayRunWindow.NewRegionalPayRunWindow("code-496", "label-294");
        item.Status = RegionalPayRunWindowStatus.Retired;
        Check.Fails(() => item.MoveTo(RegionalPayRunWindowStatus.Created), "move");
    }

    public static void RunAll()
    {
        RegionalPayRunWindowStartsInCreated();
        RegionalPayRunWindowMovesToValidated();
        RegionalPayRunWindowRejectsMovesFromRetired();
    }
}
