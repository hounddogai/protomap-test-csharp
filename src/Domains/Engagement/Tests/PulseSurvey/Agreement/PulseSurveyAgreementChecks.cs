namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Agreement;

using Acme.Workforce.Engagement.PulseSurvey.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class PulseSurveyAgreementChecks
{
    public static void PulseSurveyAgreementStartsInPlanned()
    {
        var item = PulseSurveyAgreement.NewPulseSurveyAgreement("code-22", "title-778");
        Check.Equal(item.Status, PulseSurveyAgreementStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyAgreementMovesToScheduled()
    {
        var item = PulseSurveyAgreement.NewPulseSurveyAgreement("code-22", "title-778");
        try
        {
            item.MoveTo(PulseSurveyAgreementStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyAgreementStatus.Scheduled, "status");
    }

    public static void PulseSurveyAgreementRejectsMovesFromCancelled()
    {
        var item = PulseSurveyAgreement.NewPulseSurveyAgreement("code-22", "title-778");
        item.Status = PulseSurveyAgreementStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PulseSurveyAgreementStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PulseSurveyAgreementStartsInPlanned();
        PulseSurveyAgreementMovesToScheduled();
        PulseSurveyAgreementRejectsMovesFromCancelled();
    }
}
