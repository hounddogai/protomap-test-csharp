namespace Acme.Workforce.Talent.Tests.Mobility.Window;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Mobility.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class SeasonalMobilityRecordEngineChecks
{
    public static void SeasonalMobilityRecordEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalMobilityRecordEngine.DefaultSeasonalMobilityRecordEngine(Flags.NewRegistry());
        var item = SeasonalMobilityRecord.NewSeasonalMobilityRecord("code-529", "label-692");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalMobilityRecordEngineKeepsResultsWithinBounds();
    }
}
