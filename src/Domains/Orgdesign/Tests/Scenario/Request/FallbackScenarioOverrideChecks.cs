namespace Acme.Workforce.Orgdesign.Tests.Scenario.Request;

using Acme.Workforce.Orgdesign.Scenario.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class FallbackScenarioOverrideChecks
{
    public static void FallbackScenarioOverrideStartsInRequested()
    {
        var item = FallbackScenarioOverride.NewFallbackScenarioOverride("code-472", "name-285");
        Check.Equal(item.Status, FallbackScenarioOverrideStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackScenarioOverrideMovesToGranted()
    {
        var item = FallbackScenarioOverride.NewFallbackScenarioOverride("code-472", "name-285");
        try
        {
            item.MoveTo(FallbackScenarioOverrideStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackScenarioOverrideStatus.Granted, "status");
    }

    public static void FallbackScenarioOverrideRejectsMovesFromExpired()
    {
        var item = FallbackScenarioOverride.NewFallbackScenarioOverride("code-472", "name-285");
        item.Status = FallbackScenarioOverrideStatus.Expired;
        Check.Fails(() => item.MoveTo(FallbackScenarioOverrideStatus.Requested), "move");
    }

    public static void RunAll()
    {
        FallbackScenarioOverrideStartsInRequested();
        FallbackScenarioOverrideMovesToGranted();
        FallbackScenarioOverrideRejectsMovesFromExpired();
    }
}
