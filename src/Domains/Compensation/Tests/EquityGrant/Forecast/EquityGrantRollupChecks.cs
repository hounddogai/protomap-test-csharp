namespace Acme.Workforce.Compensation.Tests.EquityGrant.Forecast;

using Acme.Workforce.Compensation.EquityGrant.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class EquityGrantRollupChecks
{
    public static void EquityGrantRollupStartsInPlanned()
    {
        var item = EquityGrantRollup.NewEquityGrantRollup("code-624", "name-214");
        Check.Equal(item.Status, EquityGrantRollupStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantRollupMovesToInProgress()
    {
        var item = EquityGrantRollup.NewEquityGrantRollup("code-624", "name-214");
        try
        {
            item.MoveTo(EquityGrantRollupStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantRollupStatus.InProgress, "status");
    }

    public static void EquityGrantRollupRejectsMovesFromCancelled()
    {
        var item = EquityGrantRollup.NewEquityGrantRollup("code-624", "name-214");
        item.Status = EquityGrantRollupStatus.Cancelled;
        Check.Fails(() => item.MoveTo(EquityGrantRollupStatus.Planned), "move");
    }

    public static void RunAll()
    {
        EquityGrantRollupStartsInPlanned();
        EquityGrantRollupMovesToInProgress();
        EquityGrantRollupRejectsMovesFromCancelled();
    }
}
