namespace Acme.Workforce.Talent.Tests.Mobility.MobilityReservation;

using Acme.Workforce.Talent.Mobility.MobilityReservation;

/// <summary>Checks of mobility reservation code, run by the project's runner.</summary>
public static class CappedMobilityOverrideChecks
{
    public static void CappedMobilityOverrideStartsInIdle()
    {
        var item = CappedMobilityOverride.NewCappedMobilityOverride("code-592", "title-148");
        Check.Equal(item.Status, CappedMobilityOverrideStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedMobilityOverrideMovesToWarming()
    {
        var item = CappedMobilityOverride.NewCappedMobilityOverride("code-592", "title-148");
        try
        {
            item.MoveTo(CappedMobilityOverrideStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedMobilityOverrideStatus.Warming, "status");
    }

    public static void CappedMobilityOverrideRejectsMovesFromStopped()
    {
        var item = CappedMobilityOverride.NewCappedMobilityOverride("code-592", "title-148");
        item.Status = CappedMobilityOverrideStatus.Stopped;
        Check.Fails(() => item.MoveTo(CappedMobilityOverrideStatus.Idle), "move");
    }

    public static void RunAll()
    {
        CappedMobilityOverrideStartsInIdle();
        CappedMobilityOverrideMovesToWarming();
        CappedMobilityOverrideRejectsMovesFromStopped();
    }
}
