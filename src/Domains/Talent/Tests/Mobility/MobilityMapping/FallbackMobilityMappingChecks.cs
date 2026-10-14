namespace Acme.Workforce.Talent.Tests.Mobility.MobilityMapping;

using Acme.Workforce.Talent.Mobility.MobilityMapping;

/// <summary>Checks of mobility mapping code, run by the project's runner.</summary>
public static class FallbackMobilityMappingChecks
{
    public static void FallbackMobilityMappingStartsInPlanned()
    {
        var item = FallbackMobilityMapping.NewFallbackMobilityMapping("code-696", "label-238");
        Check.Equal(item.Status, FallbackMobilityMappingStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackMobilityMappingMovesToCancelled()
    {
        var item = FallbackMobilityMapping.NewFallbackMobilityMapping("code-696", "label-238");
        try
        {
            item.MoveTo(FallbackMobilityMappingStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackMobilityMappingStatus.Cancelled, "status");
    }

    public static void FallbackMobilityMappingRejectsMovesFromCancelled()
    {
        var item = FallbackMobilityMapping.NewFallbackMobilityMapping("code-696", "label-238");
        item.Status = FallbackMobilityMappingStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FallbackMobilityMappingStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FallbackMobilityMappingStartsInPlanned();
        FallbackMobilityMappingMovesToCancelled();
        FallbackMobilityMappingRejectsMovesFromCancelled();
    }
}
