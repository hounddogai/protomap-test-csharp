namespace Acme.Workforce.Contractors.Tests.Rate.Plan;

using Acme.Workforce.Contractors.Rate.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RatePlanChecks
{
    public static void RatePlanStartsInNew()
    {
        var item = RatePlan.NewRatePlan("code-63", "label-212");
        Check.Equal(item.Status, RatePlanStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RatePlanMovesToDone()
    {
        var item = RatePlan.NewRatePlan("code-63", "label-212");
        try
        {
            item.MoveTo(RatePlanStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RatePlanStatus.Done, "status");
    }

    public static void RatePlanRejectsMovesFromFailed()
    {
        var item = RatePlan.NewRatePlan("code-63", "label-212");
        item.Status = RatePlanStatus.Failed;
        Check.Fails(() => item.MoveTo(RatePlanStatus.New), "move");
    }

    public static void RunAll()
    {
        RatePlanStartsInNew();
        RatePlanMovesToDone();
        RatePlanRejectsMovesFromFailed();
    }
}
