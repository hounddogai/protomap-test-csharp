namespace Acme.Workforce.Engagement.Tests.Feedback.Reservation;

using Acme.Workforce.Engagement.Feedback.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class ScheduledFeedbackRecordEngineChecks
{
    public static void ScheduledFeedbackRecordEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledFeedbackRecordEngine.DefaultScheduledFeedbackRecordEngine();
        var item = ScheduledFeedbackRecord.NewScheduledFeedbackRecord("code-702", "label-142");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledFeedbackRecordEngineKeepsResultsWithinBounds();
    }
}
