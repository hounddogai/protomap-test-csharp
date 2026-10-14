namespace Acme.Workforce.Compensation.Tests.BonusPool.Entry;

using Acme.Workforce.Compensation.BonusPool.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BonusPoolEntryEngineChecks
{
    public static void BonusPoolEntryEngineKeepsResultsWithinBounds()
    {
        var engine = BonusPoolEntryEngine.DefaultBonusPoolEntryEngine();
        var item = BonusPoolEntry.NewBonusPoolEntry("code-279", "name-136");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BonusPoolEntryEngineKeepsResultsWithinBounds();
    }
}
