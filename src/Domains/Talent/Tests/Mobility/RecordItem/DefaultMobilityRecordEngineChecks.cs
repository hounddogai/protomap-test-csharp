namespace Acme.Workforce.Talent.Tests.Mobility.RecordItem;

using Acme.Workforce.Talent.Mobility.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class DefaultMobilityRecordEngineChecks
{
    public static void DefaultMobilityRecordEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultMobilityRecordEngine.DefaultDefaultMobilityRecordEngine();
        var item = DefaultMobilityRecord.NewDefaultMobilityRecord("code-121", "label-14");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultMobilityRecordEngineKeepsResultsWithinBounds();
    }
}
