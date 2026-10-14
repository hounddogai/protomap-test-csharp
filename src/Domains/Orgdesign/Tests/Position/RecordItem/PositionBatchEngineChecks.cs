namespace Acme.Workforce.Orgdesign.Tests.Position.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class PositionBatchEngineChecks
{
    public static void PositionBatchEngineKeepsResultsWithinBounds()
    {
        var engine = PositionBatchEngine.DefaultPositionBatchEngine(Flags.NewRegistry());
        var item = PositionBatch.NewPositionBatch("code-876", "title-324");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PositionBatchEngineKeepsResultsWithinBounds();
    }
}
