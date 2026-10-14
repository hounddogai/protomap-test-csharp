namespace Acme.Workforce.Compensation.Tests.BonusPool.Rollup;

using Acme.Workforce.Compensation.BonusPool.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class ManualBonusPoolDraftEngineChecks
{
    public static void ManualBonusPoolDraftEngineKeepsResultsWithinBounds()
    {
        var engine = ManualBonusPoolDraftEngine.DefaultManualBonusPoolDraftEngine();
        var item = ManualBonusPoolDraft.NewManualBonusPoolDraft("code-986", "name-756");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualBonusPoolDraftEngineKeepsResultsWithinBounds();
    }
}
