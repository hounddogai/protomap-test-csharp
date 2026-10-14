namespace Acme.Workforce.Workplace.Tests.Visitor.Cycle;

using Acme.Workforce.Workplace.Visitor.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AdaptiveVisitorCycleChecks
{
    public static void AdaptiveVisitorCycleStartsInCreated()
    {
        var item = AdaptiveVisitorCycle.NewAdaptiveVisitorCycle("code-891", "title-896");
        Check.Equal(item.Status, AdaptiveVisitorCycleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveVisitorCycleMovesToPublished()
    {
        var item = AdaptiveVisitorCycle.NewAdaptiveVisitorCycle("code-891", "title-896");
        try
        {
            item.MoveTo(AdaptiveVisitorCycleStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveVisitorCycleStatus.Published, "status");
    }

    public static void AdaptiveVisitorCycleRejectsMovesFromRetired()
    {
        var item = AdaptiveVisitorCycle.NewAdaptiveVisitorCycle("code-891", "title-896");
        item.Status = AdaptiveVisitorCycleStatus.Retired;
        Check.Fails(() => item.MoveTo(AdaptiveVisitorCycleStatus.Created), "move");
    }

    public static void RunAll()
    {
        AdaptiveVisitorCycleStartsInCreated();
        AdaptiveVisitorCycleMovesToPublished();
        AdaptiveVisitorCycleRejectsMovesFromRetired();
    }
}
