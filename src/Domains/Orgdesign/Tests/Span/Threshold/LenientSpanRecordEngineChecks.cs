namespace Acme.Workforce.Orgdesign.Tests.Span.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LenientSpanRecordEngineChecks
{
    public static void LenientSpanRecordEngineKeepsResultsWithinBounds()
    {
        var engine = LenientSpanRecordEngine.DefaultLenientSpanRecordEngine(Flags.NewRegistry());
        var item = LenientSpanRecord.NewLenientSpanRecord("code-724", "label-700");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientSpanRecordEngineKeepsResultsWithinBounds();
    }
}
