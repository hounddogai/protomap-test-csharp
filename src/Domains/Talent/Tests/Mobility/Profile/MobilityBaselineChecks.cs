namespace Acme.Workforce.Talent.Tests.Mobility.Profile;

using Acme.Workforce.Talent.Mobility.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class MobilityBaselineChecks
{
    public static void MobilityBaselineStartsInIdle()
    {
        var item = MobilityBaseline.NewMobilityBaseline("code-16", "title-9");
        Check.Equal(item.Status, MobilityBaselineStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityBaselineMovesToReady()
    {
        var item = MobilityBaseline.NewMobilityBaseline("code-16", "title-9");
        try
        {
            item.MoveTo(MobilityBaselineStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityBaselineStatus.Ready, "status");
    }

    public static void MobilityBaselineRejectsMovesFromStopped()
    {
        var item = MobilityBaseline.NewMobilityBaseline("code-16", "title-9");
        item.Status = MobilityBaselineStatus.Stopped;
        Check.Fails(() => item.MoveTo(MobilityBaselineStatus.Idle), "move");
    }

    public static void RunAll()
    {
        MobilityBaselineStartsInIdle();
        MobilityBaselineMovesToReady();
        MobilityBaselineRejectsMovesFromStopped();
    }
}
