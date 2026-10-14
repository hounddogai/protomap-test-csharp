namespace Acme.Workforce.Compensation.Tests.BonusPool.Window;

using Acme.Workforce.Compensation.BonusPool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class BonusPoolClaimEngineChecks
{
    public static void BonusPoolClaimEngineKeepsResultsWithinBounds()
    {
        var engine = BonusPoolClaimEngine.DefaultBonusPoolClaimEngine();
        var item = BonusPoolClaim.NewBonusPoolClaim("code-830", "name-111");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BonusPoolClaimEngineKeepsResultsWithinBounds();
    }
}
