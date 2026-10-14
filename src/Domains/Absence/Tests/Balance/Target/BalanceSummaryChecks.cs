namespace Acme.Workforce.Absence.Tests.Balance.Target;

using Acme.Workforce.Absence.Balance.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class BalanceSummaryChecks
{
    public static void BalanceSummaryStartsInRequested()
    {
        var item = BalanceSummary.NewBalanceSummary("code-165", "name-165");
        Check.Equal(item.Status, BalanceSummaryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceSummaryMovesToExpired()
    {
        var item = BalanceSummary.NewBalanceSummary("code-165", "name-165");
        try
        {
            item.MoveTo(BalanceSummaryStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceSummaryStatus.Expired, "status");
    }

    public static void BalanceSummaryRejectsMovesFromExpired()
    {
        var item = BalanceSummary.NewBalanceSummary("code-165", "name-165");
        item.Status = BalanceSummaryStatus.Expired;
        Check.Fails(() => item.MoveTo(BalanceSummaryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BalanceSummaryStartsInRequested();
        BalanceSummaryMovesToExpired();
        BalanceSummaryRejectsMovesFromExpired();
    }
}
