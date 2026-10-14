namespace Acme.Workforce.Engagement.Tests.Feedback.Manifest;

using Acme.Workforce.Engagement.Feedback.Manifest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FeedbackManifestEngineChecks
{
    public static void FeedbackManifestEngineKeepsResultsWithinBounds()
    {
        var engine = FeedbackManifestEngine.DefaultFeedbackManifestEngine(Flags.NewRegistry());
        var item = FeedbackManifest.NewFeedbackManifest("code-111", "name-730");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FeedbackManifestEngineKeepsResultsWithinBounds();
    }
}
