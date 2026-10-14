namespace Acme.Workforce.Talent.Tests.Mobility.OverrideItem;

using Acme.Workforce.Talent.Mobility.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class StrictMobilityOverrideChecks
{
    public static void StrictMobilityOverrideStartsInRequested()
    {
        var item = StrictMobilityOverride.NewStrictMobilityOverride("code-244", "title-456");
        Check.Equal(item.Status, StrictMobilityOverrideStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictMobilityOverrideMovesToGranted()
    {
        var item = StrictMobilityOverride.NewStrictMobilityOverride("code-244", "title-456");
        try
        {
            item.MoveTo(StrictMobilityOverrideStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictMobilityOverrideStatus.Granted, "status");
    }

    public static void StrictMobilityOverrideRejectsMovesFromExpired()
    {
        var item = StrictMobilityOverride.NewStrictMobilityOverride("code-244", "title-456");
        item.Status = StrictMobilityOverrideStatus.Expired;
        Check.Fails(() => item.MoveTo(StrictMobilityOverrideStatus.Requested), "move");
    }

    public static void RunAll()
    {
        StrictMobilityOverrideStartsInRequested();
        StrictMobilityOverrideMovesToGranted();
        StrictMobilityOverrideRejectsMovesFromExpired();
    }
}
