namespace Acme.Workforce.Engagement.Tests.Feedback.Slot;

using Acme.Workforce.Engagement.Feedback.Slot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class TieredFeedbackPolicyEngineChecks
{
    public static void TieredFeedbackPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = TieredFeedbackPolicyEngine.DefaultTieredFeedbackPolicyEngine(Flags.NewRegistry());
        var item = TieredFeedbackPolicy.NewTieredFeedbackPolicy("code-563", "title-669");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredFeedbackPolicyEngineKeepsResultsWithinBounds();
    }
}
