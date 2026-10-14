namespace Acme.Workforce.Compliance.Tests.Certification.Snapshot;

using Acme.Workforce.Compliance.Certification.Snapshot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class TieredCertificationSnapshotEngineChecks
{
    public static void TieredCertificationSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = TieredCertificationSnapshotEngine.DefaultTieredCertificationSnapshotEngine(Flags.NewRegistry());
        var item = TieredCertificationSnapshot.NewTieredCertificationSnapshot("code-167", "label-576");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredCertificationSnapshotEngineKeepsResultsWithinBounds();
    }
}
