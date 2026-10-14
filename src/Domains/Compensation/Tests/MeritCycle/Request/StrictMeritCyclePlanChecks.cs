namespace Acme.Workforce.Compensation.Tests.MeritCycle.Request;

using Acme.Workforce.Compensation.MeritCycle.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class StrictMeritCyclePlanChecks
{
    public static void StrictMeritCyclePlanStartsInOpen()
    {
        var item = StrictMeritCyclePlan.NewStrictMeritCyclePlan("code-180", "name-81");
        Check.Equal(item.Status, StrictMeritCyclePlanStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictMeritCyclePlanMovesToInReview()
    {
        var item = StrictMeritCyclePlan.NewStrictMeritCyclePlan("code-180", "name-81");
        try
        {
            item.MoveTo(StrictMeritCyclePlanStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictMeritCyclePlanStatus.InReview, "status");
    }

    public static void StrictMeritCyclePlanRejectsMovesFromArchived()
    {
        var item = StrictMeritCyclePlan.NewStrictMeritCyclePlan("code-180", "name-81");
        item.Status = StrictMeritCyclePlanStatus.Archived;
        Check.Fails(() => item.MoveTo(StrictMeritCyclePlanStatus.Open), "move");
    }

    public static void RunAll()
    {
        StrictMeritCyclePlanStartsInOpen();
        StrictMeritCyclePlanMovesToInReview();
        StrictMeritCyclePlanRejectsMovesFromArchived();
    }
}
