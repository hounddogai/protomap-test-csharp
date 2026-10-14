namespace Acme.Workforce.Engagement.Tests.Feedback.Quota;

using Acme.Workforce.Engagement.Feedback.Quota;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class StrictFeedbackQuotaEngineChecks
{
    public static void StrictFeedbackQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = StrictFeedbackQuotaEngine.DefaultStrictFeedbackQuotaEngine(Flags.NewRegistry());
        var item = StrictFeedbackQuota.NewStrictFeedbackQuota("code-716", "title-885");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictFeedbackQuotaEngineKeepsResultsWithinBounds();
    }
}
