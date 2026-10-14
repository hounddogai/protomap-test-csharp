namespace Acme.Workforce.Contractors.Tests.Engagement.Tier;

using Acme.Workforce.Contractors.Engagement.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class EngagementTierEngineChecks
{
    public static void EngagementTierEngineKeepsResultsWithinBounds()
    {
        var engine = EngagementTierEngine.DefaultEngagementTierEngine();
        var item = EngagementTier.NewEngagementTier("code-516", "label-748");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EngagementTierEngineKeepsResultsWithinBounds();
    }
}
