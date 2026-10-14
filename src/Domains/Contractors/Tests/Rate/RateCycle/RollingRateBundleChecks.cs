namespace Acme.Workforce.Contractors.Tests.Rate.RateCycle;

using Acme.Workforce.Contractors.Rate.RateCycle;

/// <summary>Checks of rate cycle code, run by the project's runner.</summary>
public static class RollingRateBundleChecks
{
    public static void RollingRateBundleStartsInDraft()
    {
        var item = RollingRateBundle.NewRollingRateBundle("code-508", "name-918");
        Check.Equal(item.Status, RollingRateBundleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingRateBundleMovesToPending()
    {
        var item = RollingRateBundle.NewRollingRateBundle("code-508", "name-918");
        try
        {
            item.MoveTo(RollingRateBundleStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingRateBundleStatus.Pending, "status");
    }

    public static void RollingRateBundleRejectsMovesFromClosed()
    {
        var item = RollingRateBundle.NewRollingRateBundle("code-508", "name-918");
        item.Status = RollingRateBundleStatus.Closed;
        Check.Fails(() => item.MoveTo(RollingRateBundleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RollingRateBundleStartsInDraft();
        RollingRateBundleMovesToPending();
        RollingRateBundleRejectsMovesFromClosed();
    }
}
