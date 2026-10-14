namespace Acme.Workforce.Engagement.Tests.TownHall.Claim;

using Acme.Workforce.Engagement.TownHall.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class TownHallQuotaEngineChecks
{
    public static void TownHallQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = TownHallQuotaEngine.DefaultTownHallQuotaEngine();
        var item = TownHallQuota.NewTownHallQuota("code-628", "title-753");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TownHallQuotaEngineKeepsResultsWithinBounds();
    }
}
