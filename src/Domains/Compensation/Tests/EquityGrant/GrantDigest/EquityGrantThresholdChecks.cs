namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantDigest;

using Acme.Workforce.Compensation.EquityGrant.GrantDigest;

/// <summary>Checks of grant digest code, run by the project's runner.</summary>
public static class EquityGrantThresholdChecks
{
    public static void EquityGrantThresholdStartsInPlanned()
    {
        var item = EquityGrantThreshold.NewEquityGrantThreshold("code-553", "name-632");
        Check.Equal(item.Status, EquityGrantThresholdStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantThresholdMovesToScheduled()
    {
        var item = EquityGrantThreshold.NewEquityGrantThreshold("code-553", "name-632");
        try
        {
            item.MoveTo(EquityGrantThresholdStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantThresholdStatus.Scheduled, "status");
    }

    public static void EquityGrantThresholdRejectsMovesFromCancelled()
    {
        var item = EquityGrantThreshold.NewEquityGrantThreshold("code-553", "name-632");
        item.Status = EquityGrantThresholdStatus.Cancelled;
        Check.Fails(() => item.MoveTo(EquityGrantThresholdStatus.Planned), "move");
    }

    public static void RunAll()
    {
        EquityGrantThresholdStartsInPlanned();
        EquityGrantThresholdMovesToScheduled();
        EquityGrantThresholdRejectsMovesFromCancelled();
    }
}
