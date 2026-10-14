namespace Acme.Workforce.Workplace.Tests.Visitor.Score;

using Acme.Workforce.Workplace.Visitor.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class AdaptiveVisitorOverrideChecks
{
    public static void AdaptiveVisitorOverrideStartsInRequested()
    {
        var item = AdaptiveVisitorOverride.NewAdaptiveVisitorOverride("code-496", "name-747");
        Check.Equal(item.Status, AdaptiveVisitorOverrideStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveVisitorOverrideMovesToExpired()
    {
        var item = AdaptiveVisitorOverride.NewAdaptiveVisitorOverride("code-496", "name-747");
        try
        {
            item.MoveTo(AdaptiveVisitorOverrideStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveVisitorOverrideStatus.Expired, "status");
    }

    public static void AdaptiveVisitorOverrideRejectsMovesFromExpired()
    {
        var item = AdaptiveVisitorOverride.NewAdaptiveVisitorOverride("code-496", "name-747");
        item.Status = AdaptiveVisitorOverrideStatus.Expired;
        Check.Fails(() => item.MoveTo(AdaptiveVisitorOverrideStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AdaptiveVisitorOverrideStartsInRequested();
        AdaptiveVisitorOverrideMovesToExpired();
        AdaptiveVisitorOverrideRejectsMovesFromExpired();
    }
}
