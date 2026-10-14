namespace Acme.Workforce.Compensation.Tests.BonusPool.Batch;

using Acme.Workforce.Compensation.BonusPool.Batch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BonusPoolProfileEngineChecks
{
    public static void BonusPoolProfileEngineKeepsResultsWithinBounds()
    {
        var engine = BonusPoolProfileEngine.DefaultBonusPoolProfileEngine(Flags.NewRegistry());
        var item = BonusPoolProfile.NewBonusPoolProfile("code-273", "name-914");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BonusPoolProfileEngineKeepsResultsWithinBounds();
    }
}
