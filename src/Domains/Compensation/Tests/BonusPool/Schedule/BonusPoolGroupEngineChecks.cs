namespace Acme.Workforce.Compensation.Tests.BonusPool.Schedule;

using Acme.Workforce.Compensation.BonusPool.Schedule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class BonusPoolGroupEngineChecks
{
    public static void BonusPoolGroupEngineKeepsResultsWithinBounds()
    {
        var engine = BonusPoolGroupEngine.DefaultBonusPoolGroupEngine(Flags.NewRegistry());
        var item = BonusPoolGroup.NewBonusPoolGroup("code-741", "label-824");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BonusPoolGroupEngineKeepsResultsWithinBounds();
    }
}
