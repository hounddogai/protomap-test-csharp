namespace Acme.Workforce.Contractors.Tests.Rate.RecordItem;

using Acme.Workforce.Contractors.Rate.RecordItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RateRecordEngineChecks
{
    public static void RateRecordEngineKeepsResultsWithinBounds()
    {
        var engine = RateRecordEngine.DefaultRateRecordEngine(Flags.NewRegistry());
        var item = RateRecord.NewRateRecord("code-196", "title-928");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RateRecordEngineKeepsResultsWithinBounds();
    }
}
