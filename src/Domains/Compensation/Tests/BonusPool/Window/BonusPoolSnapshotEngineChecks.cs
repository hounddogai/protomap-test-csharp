namespace Acme.Workforce.Compensation.Tests.BonusPool.Window;

using Acme.Workforce.Compensation.BonusPool.Window;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class BonusPoolSnapshotEngineChecks
{
    public static void BonusPoolSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = BonusPoolSnapshotEngine.DefaultBonusPoolSnapshotEngine(Flags.NewRegistry());
        var item = BonusPoolSnapshot.NewBonusPoolSnapshot("code-567", "name-611");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BonusPoolSnapshotEngineKeepsResultsWithinBounds();
    }
}
