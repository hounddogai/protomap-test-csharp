namespace Acme.Workforce.Talent.Tests.Mobility.MobilityRecord;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Mobility.MobilityRecord;

/// <summary>Checks of mobility record code, run by the project's runner.</summary>
public static class WeightedMobilityRecordEngineChecks
{
    public static void WeightedMobilityRecordEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedMobilityRecordEngine.DefaultWeightedMobilityRecordEngine(Flags.NewRegistry());
        var item = WeightedMobilityRecord.NewWeightedMobilityRecord("code-479", "name-285");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedMobilityRecordEngineKeepsResultsWithinBounds();
    }
}
