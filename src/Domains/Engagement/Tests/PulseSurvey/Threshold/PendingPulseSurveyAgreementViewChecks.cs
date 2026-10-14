namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Threshold;

using Acme.Workforce.Engagement.PulseSurvey.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PendingPulseSurveyAgreementViewChecks
{
    public static void PendingPulseSurveyAgreementViewShowsTheStatusInCapitals()
    {
        var item = PendingPulseSurveyAgreement.NewPendingPulseSurveyAgreement("code-308", "title-461");
        var view = PendingPulseSurveyAgreementView.ToPendingPulseSurveyAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingPulseSurveyAgreementView.ToPendingPulseSurveyAgreementViews(new List<PendingPulseSurveyAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingPulseSurveyAgreementViewShowsTheStatusInCapitals();
    }
}
