namespace Acme.Workforce.Absence.Tests.Balance.Threshold;

using Acme.Workforce.Absence.Balance.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PrimaryBalanceMetricChecks
{
    public static void PrimaryBalanceMetricStartsInPlanned()
    {
        var item = PrimaryBalanceMetric.NewPrimaryBalanceMetric("code-58", "title-534");
        Check.Equal(item.Status, PrimaryBalanceMetricStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryBalanceMetricMovesToCancelled()
    {
        var item = PrimaryBalanceMetric.NewPrimaryBalanceMetric("code-58", "title-534");
        try
        {
            item.MoveTo(PrimaryBalanceMetricStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryBalanceMetricStatus.Cancelled, "status");
    }

    public static void PrimaryBalanceMetricRejectsMovesFromCancelled()
    {
        var item = PrimaryBalanceMetric.NewPrimaryBalanceMetric("code-58", "title-534");
        item.Status = PrimaryBalanceMetricStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PrimaryBalanceMetricStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PrimaryBalanceMetricStartsInPlanned();
        PrimaryBalanceMetricMovesToCancelled();
        PrimaryBalanceMetricRejectsMovesFromCancelled();
    }
}
