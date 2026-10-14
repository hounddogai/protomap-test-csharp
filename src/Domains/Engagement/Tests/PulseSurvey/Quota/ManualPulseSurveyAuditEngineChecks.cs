namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Quota;

using Acme.Workforce.Engagement.PulseSurvey.Quota;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class ManualPulseSurveyAuditEngineChecks
{
    public static void ManualPulseSurveyAuditEngineKeepsResultsWithinBounds()
    {
        var engine = ManualPulseSurveyAuditEngine.DefaultManualPulseSurveyAuditEngine(Flags.NewRegistry());
        var item = ManualPulseSurveyAudit.NewManualPulseSurveyAudit("code-444", "label-295");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualPulseSurveyAuditEngineKeepsResultsWithinBounds();
    }
}
