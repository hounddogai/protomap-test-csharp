namespace Acme.Workforce.Payrollops.Tests.Deduction.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class StrictDeductionSegmentEngineChecks
{
    public static void StrictDeductionSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = StrictDeductionSegmentEngine.DefaultStrictDeductionSegmentEngine(Flags.NewRegistry());
        var item = StrictDeductionSegment.NewStrictDeductionSegment("code-200", "name-78");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictDeductionSegmentEngineKeepsResultsWithinBounds();
    }
}
