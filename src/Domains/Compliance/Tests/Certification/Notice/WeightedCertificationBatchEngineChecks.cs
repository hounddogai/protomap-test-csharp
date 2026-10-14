namespace Acme.Workforce.Compliance.Tests.Certification.Notice;

using Acme.Workforce.Compliance.Certification.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class WeightedCertificationBatchEngineChecks
{
    public static void WeightedCertificationBatchEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedCertificationBatchEngine.DefaultWeightedCertificationBatchEngine();
        var item = WeightedCertificationBatch.NewWeightedCertificationBatch("code-963", "title-448");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedCertificationBatchEngineKeepsResultsWithinBounds();
    }
}
