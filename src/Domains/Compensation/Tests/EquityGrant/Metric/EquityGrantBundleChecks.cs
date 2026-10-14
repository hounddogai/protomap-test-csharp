namespace Acme.Workforce.Compensation.Tests.EquityGrant.Metric;

using Acme.Workforce.Compensation.EquityGrant.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class EquityGrantBundleChecks
{
    public static void EquityGrantBundleStartsInDraft()
    {
        var item = EquityGrantBundle.NewEquityGrantBundle("code-907", "label-633");
        Check.Equal(item.Status, EquityGrantBundleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantBundleMovesToSuspended()
    {
        var item = EquityGrantBundle.NewEquityGrantBundle("code-907", "label-633");
        try
        {
            item.MoveTo(EquityGrantBundleStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantBundleStatus.Suspended, "status");
    }

    public static void EquityGrantBundleRejectsMovesFromClosed()
    {
        var item = EquityGrantBundle.NewEquityGrantBundle("code-907", "label-633");
        item.Status = EquityGrantBundleStatus.Closed;
        Check.Fails(() => item.MoveTo(EquityGrantBundleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        EquityGrantBundleStartsInDraft();
        EquityGrantBundleMovesToSuspended();
        EquityGrantBundleRejectsMovesFromClosed();
    }
}
