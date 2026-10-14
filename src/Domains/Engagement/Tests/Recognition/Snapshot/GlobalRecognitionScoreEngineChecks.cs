namespace Acme.Workforce.Engagement.Tests.Recognition.Snapshot;

using Acme.Workforce.Engagement.Recognition.Snapshot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class GlobalRecognitionScoreEngineChecks
{
    public static void GlobalRecognitionScoreEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalRecognitionScoreEngine.DefaultGlobalRecognitionScoreEngine(Flags.NewRegistry());
        var item = GlobalRecognitionScore.NewGlobalRecognitionScore("code-701", "title-90");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalRecognitionScoreEngineKeepsResultsWithinBounds();
    }
}
