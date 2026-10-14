namespace Acme.Workforce.Engagement.Tests.Feedback.Notice;

using Acme.Workforce.Engagement.Feedback.Notice;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PrimaryFeedbackNoticeEngineChecks
{
    public static void PrimaryFeedbackNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryFeedbackNoticeEngine.DefaultPrimaryFeedbackNoticeEngine(Flags.NewRegistry());
        var item = PrimaryFeedbackNotice.NewPrimaryFeedbackNotice("code-100", "title-935");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryFeedbackNoticeEngineKeepsResultsWithinBounds();
    }
}
