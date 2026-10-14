namespace Acme.Workforce.Talent.Tests.Mobility.Audit;

using Acme.Workforce.Talent.Mobility.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LegacyMobilitySegmentChecks
{
    public static void LegacyMobilitySegmentStartsInIdle()
    {
        var item = LegacyMobilitySegment.NewLegacyMobilitySegment("code-28", "title-796");
        Check.Equal(item.Status, LegacyMobilitySegmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyMobilitySegmentMovesToWarming()
    {
        var item = LegacyMobilitySegment.NewLegacyMobilitySegment("code-28", "title-796");
        try
        {
            item.MoveTo(LegacyMobilitySegmentStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyMobilitySegmentStatus.Warming, "status");
    }

    public static void LegacyMobilitySegmentRejectsMovesFromStopped()
    {
        var item = LegacyMobilitySegment.NewLegacyMobilitySegment("code-28", "title-796");
        item.Status = LegacyMobilitySegmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(LegacyMobilitySegmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LegacyMobilitySegmentStartsInIdle();
        LegacyMobilitySegmentMovesToWarming();
        LegacyMobilitySegmentRejectsMovesFromStopped();
    }
}
