namespace Acme.Workforce.Compensation.Tests.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of primary pool schedule code, run by the project's runner.</summary>
public static class PendingBonusPoolPolicyEngineChecks
{
    public static void PendingBonusPoolPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = PendingBonusPoolPolicyEngine.DefaultPendingBonusPoolPolicyEngine(Flags.NewRegistry());
        var item = PendingBonusPoolPolicy.NewPendingBonusPoolPolicy("code-601", "title-236");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingBonusPoolPolicyEngineKeepsResultsWithinBounds();
    }
}
