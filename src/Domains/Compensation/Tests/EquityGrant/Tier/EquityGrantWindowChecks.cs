namespace Acme.Workforce.Compensation.Tests.EquityGrant.Tier;

using Acme.Workforce.Compensation.EquityGrant.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class EquityGrantWindowChecks
{
    public static void EquityGrantWindowStartsInIdle()
    {
        var item = EquityGrantWindow.NewEquityGrantWindow("code-734", "label-585");
        Check.Equal(item.Status, EquityGrantWindowStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantWindowMovesToWarming()
    {
        var item = EquityGrantWindow.NewEquityGrantWindow("code-734", "label-585");
        try
        {
            item.MoveTo(EquityGrantWindowStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantWindowStatus.Warming, "status");
    }

    public static void EquityGrantWindowRejectsMovesFromStopped()
    {
        var item = EquityGrantWindow.NewEquityGrantWindow("code-734", "label-585");
        item.Status = EquityGrantWindowStatus.Stopped;
        Check.Fails(() => item.MoveTo(EquityGrantWindowStatus.Idle), "move");
    }

    public static void RunAll()
    {
        EquityGrantWindowStartsInIdle();
        EquityGrantWindowMovesToWarming();
        EquityGrantWindowRejectsMovesFromStopped();
    }
}
