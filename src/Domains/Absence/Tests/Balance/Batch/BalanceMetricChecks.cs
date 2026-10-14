namespace Acme.Workforce.Absence.Tests.Balance.Batch;

using Acme.Workforce.Absence.Balance.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BalanceMetricChecks
{
    public static void BalanceMetricStartsInRequested()
    {
        var item = BalanceMetric.NewBalanceMetric("code-626", "title-693");
        Check.Equal(item.Status, BalanceMetricStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceMetricMovesToGranted()
    {
        var item = BalanceMetric.NewBalanceMetric("code-626", "title-693");
        try
        {
            item.MoveTo(BalanceMetricStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceMetricStatus.Granted, "status");
    }

    public static void BalanceMetricRejectsMovesFromExpired()
    {
        var item = BalanceMetric.NewBalanceMetric("code-626", "title-693");
        item.Status = BalanceMetricStatus.Expired;
        Check.Fails(() => item.MoveTo(BalanceMetricStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BalanceMetricStartsInRequested();
        BalanceMetricMovesToGranted();
        BalanceMetricRejectsMovesFromExpired();
    }
}
