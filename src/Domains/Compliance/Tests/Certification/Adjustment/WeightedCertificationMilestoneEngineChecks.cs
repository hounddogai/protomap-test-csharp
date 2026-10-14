namespace Acme.Workforce.Compliance.Tests.Certification.Adjustment;

using Acme.Workforce.Compliance.Certification.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class WeightedCertificationMilestoneEngineChecks
{
    public static void WeightedCertificationMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedCertificationMilestoneEngine.DefaultWeightedCertificationMilestoneEngine();
        var item = WeightedCertificationMilestone.NewWeightedCertificationMilestone("code-347", "name-531");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedCertificationMilestoneEngineKeepsResultsWithinBounds();
    }
}
