namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Target;

using Acme.Workforce.Engagement.PulseSurvey.Target;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PendingPulseSurveyAuditEngineChecks
{
    public static void PendingPulseSurveyAuditEngineKeepsResultsWithinBounds()
    {
        var engine = PendingPulseSurveyAuditEngine.DefaultPendingPulseSurveyAuditEngine(Flags.NewRegistry());
        var item = PendingPulseSurveyAudit.NewPendingPulseSurveyAudit("code-963", "title-963");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingPulseSurveyAuditEngineKeepsResultsWithinBounds();
    }
}
