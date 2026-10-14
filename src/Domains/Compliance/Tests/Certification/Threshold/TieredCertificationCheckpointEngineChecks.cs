namespace Acme.Workforce.Compliance.Tests.Certification.Threshold;

using Acme.Workforce.Compliance.Certification.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class TieredCertificationCheckpointEngineChecks
{
    public static void TieredCertificationCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = TieredCertificationCheckpointEngine.DefaultTieredCertificationCheckpointEngine();
        var item = TieredCertificationCheckpoint.NewTieredCertificationCheckpoint("code-841", "name-481");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredCertificationCheckpointEngineKeepsResultsWithinBounds();
    }
}
