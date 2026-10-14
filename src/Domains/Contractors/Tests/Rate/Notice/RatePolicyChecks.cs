namespace Acme.Workforce.Contractors.Tests.Rate.Notice;

using Acme.Workforce.Contractors.Rate.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RatePolicyChecks
{
    public static void RatePolicyStartsInOpen()
    {
        var item = RatePolicy.NewRatePolicy("code-79", "label-446");
        Check.Equal(item.Status, RatePolicyStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RatePolicyMovesToReopened()
    {
        var item = RatePolicy.NewRatePolicy("code-79", "label-446");
        try
        {
            item.MoveTo(RatePolicyStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RatePolicyStatus.Reopened, "status");
    }

    public static void RatePolicyRejectsMovesFromArchived()
    {
        var item = RatePolicy.NewRatePolicy("code-79", "label-446");
        item.Status = RatePolicyStatus.Archived;
        Check.Fails(() => item.MoveTo(RatePolicyStatus.Open), "move");
    }

    public static void RunAll()
    {
        RatePolicyStartsInOpen();
        RatePolicyMovesToReopened();
        RatePolicyRejectsMovesFromArchived();
    }
}
