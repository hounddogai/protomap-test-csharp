namespace Acme.Workforce.Compensation.Tests.EquityGrant.Assignment;

using Acme.Workforce.Compensation.EquityGrant.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class ManualEquityGrantWindowChecks
{
    public static void ManualEquityGrantWindowStartsInDraft()
    {
        var item = ManualEquityGrantWindow.NewManualEquityGrantWindow("code-753", "title-128");
        Check.Equal(item.Status, ManualEquityGrantWindowStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualEquityGrantWindowMovesToActive()
    {
        var item = ManualEquityGrantWindow.NewManualEquityGrantWindow("code-753", "title-128");
        try
        {
            item.MoveTo(ManualEquityGrantWindowStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualEquityGrantWindowStatus.Active, "status");
    }

    public static void ManualEquityGrantWindowRejectsMovesFromClosed()
    {
        var item = ManualEquityGrantWindow.NewManualEquityGrantWindow("code-753", "title-128");
        item.Status = ManualEquityGrantWindowStatus.Closed;
        Check.Fails(() => item.MoveTo(ManualEquityGrantWindowStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ManualEquityGrantWindowStartsInDraft();
        ManualEquityGrantWindowMovesToActive();
        ManualEquityGrantWindowRejectsMovesFromClosed();
    }
}
