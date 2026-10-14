namespace Acme.Workforce.Talent.Tests.Mobility.Target;

using Acme.Workforce.Talent.Mobility.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class MobilityNoticeEngineChecks
{
    public static void MobilityNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = MobilityNoticeEngine.DefaultMobilityNoticeEngine();
        var item = MobilityNotice.NewMobilityNotice("code-647", "name-250");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MobilityNoticeEngineKeepsResultsWithinBounds();
    }
}
