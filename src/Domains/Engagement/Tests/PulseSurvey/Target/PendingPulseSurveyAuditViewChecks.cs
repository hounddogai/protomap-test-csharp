namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Target;

using Acme.Workforce.Engagement.PulseSurvey.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PendingPulseSurveyAuditViewChecks
{
    public static void PendingPulseSurveyAuditViewShowsTheStatusInCapitals()
    {
        var item = PendingPulseSurveyAudit.NewPendingPulseSurveyAudit("code-963", "title-963");
        var view = PendingPulseSurveyAuditView.ToPendingPulseSurveyAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingPulseSurveyAuditView.ToPendingPulseSurveyAuditViews(new List<PendingPulseSurveyAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingPulseSurveyAuditViewShowsTheStatusInCapitals();
    }
}
