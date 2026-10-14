namespace Acme.Workforce.Orgdesign.Tests.Headcount.HeadcountRecord;

using Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

/// <summary>Checks of headcount record code, run by the project's runner.</summary>
public static class HeadcountSnapshotEngineChecks
{
    public static void HeadcountSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = HeadcountSnapshotEngine.DefaultHeadcountSnapshotEngine();
        var item = HeadcountSnapshot.NewHeadcountSnapshot("code-96", "label-172");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HeadcountSnapshotEngineKeepsResultsWithinBounds();
    }
}
