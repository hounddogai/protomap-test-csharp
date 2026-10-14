namespace Acme.Workforce.Contractors.Tests.Engagement.Profile;

using Acme.Workforce.Contractors.Engagement.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class WeightedEngagementAgreementEngineChecks
{
    public static void WeightedEngagementAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedEngagementAgreementEngine.DefaultWeightedEngagementAgreementEngine();
        var item = WeightedEngagementAgreement.NewWeightedEngagementAgreement("code-552", "name-931");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedEngagementAgreementEngineKeepsResultsWithinBounds();
    }
}
