namespace Acme.Workforce.Engagement.Tests.Recognition.Agreement;

using Acme.Workforce.Engagement.Recognition.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class RecognitionAgreementChecks
{
    public static void RecognitionAgreementStartsInDraft()
    {
        var item = RecognitionAgreement.NewRecognitionAgreement("code-833", "label-865");
        Check.Equal(item.Status, RecognitionAgreementStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionAgreementMovesToActive()
    {
        var item = RecognitionAgreement.NewRecognitionAgreement("code-833", "label-865");
        try
        {
            item.MoveTo(RecognitionAgreementStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionAgreementStatus.Active, "status");
    }

    public static void RecognitionAgreementRejectsMovesFromClosed()
    {
        var item = RecognitionAgreement.NewRecognitionAgreement("code-833", "label-865");
        item.Status = RecognitionAgreementStatus.Closed;
        Check.Fails(() => item.MoveTo(RecognitionAgreementStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RecognitionAgreementStartsInDraft();
        RecognitionAgreementMovesToActive();
        RecognitionAgreementRejectsMovesFromClosed();
    }
}
