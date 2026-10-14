namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Quota;

using Acme.Workforce.Engagement.PulseSurvey.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class ManualPulseSurveyAuditViewChecks
{
    public static void ManualPulseSurveyAuditViewShowsTheStatusInCapitals()
    {
        var item = ManualPulseSurveyAudit.NewManualPulseSurveyAudit("code-444", "label-295");
        var view = ManualPulseSurveyAuditView.ToManualPulseSurveyAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualPulseSurveyAuditView.ToManualPulseSurveyAuditViews(new List<ManualPulseSurveyAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualPulseSurveyAuditViewShowsTheStatusInCapitals();
    }
}
