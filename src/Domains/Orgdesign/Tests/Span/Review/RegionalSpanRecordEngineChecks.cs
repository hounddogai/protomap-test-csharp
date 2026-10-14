namespace Acme.Workforce.Orgdesign.Tests.Span.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RegionalSpanRecordEngineChecks
{
    public static void RegionalSpanRecordEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalSpanRecordEngine.DefaultRegionalSpanRecordEngine(Flags.NewRegistry());
        var item = RegionalSpanRecord.NewRegionalSpanRecord("code-687", "title-403");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalSpanRecordEngineKeepsResultsWithinBounds();
    }
}
