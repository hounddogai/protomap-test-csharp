namespace Acme.Workforce.Contractors.Tests.Engagement.Assignment;

using Acme.Workforce.Contractors.Engagement.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class EngagementAgreementEngineChecks
{
    public static void EngagementAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = EngagementAgreementEngine.DefaultEngagementAgreementEngine();
        var item = EngagementAgreement.NewEngagementAgreement("code-580", "label-439");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EngagementAgreementEngineKeepsResultsWithinBounds();
    }
}
