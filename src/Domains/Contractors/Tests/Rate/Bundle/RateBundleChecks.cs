namespace Acme.Workforce.Contractors.Tests.Rate.Bundle;

using Acme.Workforce.Contractors.Rate.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class RateBundleChecks
{
    public static void RateBundleStartsInPlanned()
    {
        var item = RateBundle.NewRateBundle("code-39", "label-929");
        Check.Equal(item.Status, RateBundleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateBundleMovesToCompleted()
    {
        var item = RateBundle.NewRateBundle("code-39", "label-929");
        try
        {
            item.MoveTo(RateBundleStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateBundleStatus.Completed, "status");
    }

    public static void RateBundleRejectsMovesFromCancelled()
    {
        var item = RateBundle.NewRateBundle("code-39", "label-929");
        item.Status = RateBundleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RateBundleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RateBundleStartsInPlanned();
        RateBundleMovesToCompleted();
        RateBundleRejectsMovesFromCancelled();
    }
}
