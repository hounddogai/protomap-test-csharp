namespace Acme.Workforce.Workplace.Tests.Visitor.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class VisitorSnapshotEngineChecks
{
    public static void VisitorSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorSnapshotEngine.DefaultVisitorSnapshotEngine(Flags.NewRegistry());
        var item = VisitorSnapshot.NewVisitorSnapshot("code-520", "label-627");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorSnapshotEngineKeepsResultsWithinBounds();
    }
}
