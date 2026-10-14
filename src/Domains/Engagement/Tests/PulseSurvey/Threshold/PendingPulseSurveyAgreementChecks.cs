namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Threshold;

using Acme.Workforce.Engagement.PulseSurvey.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PendingPulseSurveyAgreementChecks
{
    public static void PendingPulseSurveyAgreementStartsInCreated()
    {
        var item = PendingPulseSurveyAgreement.NewPendingPulseSurveyAgreement("code-308", "title-461");
        Check.Equal(item.Status, PendingPulseSurveyAgreementStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingPulseSurveyAgreementMovesToValidated()
    {
        var item = PendingPulseSurveyAgreement.NewPendingPulseSurveyAgreement("code-308", "title-461");
        try
        {
            item.MoveTo(PendingPulseSurveyAgreementStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingPulseSurveyAgreementStatus.Validated, "status");
    }

    public static void PendingPulseSurveyAgreementRejectsMovesFromRetired()
    {
        var item = PendingPulseSurveyAgreement.NewPendingPulseSurveyAgreement("code-308", "title-461");
        item.Status = PendingPulseSurveyAgreementStatus.Retired;
        Check.Fails(() => item.MoveTo(PendingPulseSurveyAgreementStatus.Created), "move");
    }

    public static void RunAll()
    {
        PendingPulseSurveyAgreementStartsInCreated();
        PendingPulseSurveyAgreementMovesToValidated();
        PendingPulseSurveyAgreementRejectsMovesFromRetired();
    }
}
