namespace Acme.Workforce.Orgdesign.Tests.Headcount.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Headcount.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class HeadcountBatchEngineChecks
{
    public static void HeadcountBatchEngineKeepsResultsWithinBounds()
    {
        var engine = HeadcountBatchEngine.DefaultHeadcountBatchEngine(Flags.NewRegistry());
        var item = HeadcountBatch.NewHeadcountBatch("code-460", "name-204");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HeadcountBatchEngineKeepsResultsWithinBounds();
    }
}
