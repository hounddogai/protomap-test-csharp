namespace Acme.Workforce.Talent.Tests.Mobility.Mapping;

using Acme.Workforce.Talent.Mobility.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class MobilityMappingChecks
{
    public static void MobilityMappingStartsInPlanned()
    {
        var item = MobilityMapping.NewMobilityMapping("code-897", "title-357");
        Check.Equal(item.Status, MobilityMappingStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityMappingMovesToScheduled()
    {
        var item = MobilityMapping.NewMobilityMapping("code-897", "title-357");
        try
        {
            item.MoveTo(MobilityMappingStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityMappingStatus.Scheduled, "status");
    }

    public static void MobilityMappingRejectsMovesFromCancelled()
    {
        var item = MobilityMapping.NewMobilityMapping("code-897", "title-357");
        item.Status = MobilityMappingStatus.Cancelled;
        Check.Fails(() => item.MoveTo(MobilityMappingStatus.Planned), "move");
    }

    public static void RunAll()
    {
        MobilityMappingStartsInPlanned();
        MobilityMappingMovesToScheduled();
        MobilityMappingRejectsMovesFromCancelled();
    }
}
