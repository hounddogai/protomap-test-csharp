namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentVariance;

using Acme.Workforce.Engagement.Sentiment.SentimentVariance;

/// <summary>Checks of sentiment variance code, run by the project's runner.</summary>
public static class SentimentAgreementChecks
{
    public static void SentimentAgreementStartsInCreated()
    {
        var item = SentimentAgreement.NewSentimentAgreement("code-800", "name-912");
        Check.Equal(item.Status, SentimentAgreementStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentAgreementMovesToPublished()
    {
        var item = SentimentAgreement.NewSentimentAgreement("code-800", "name-912");
        try
        {
            item.MoveTo(SentimentAgreementStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentAgreementStatus.Published, "status");
    }

    public static void SentimentAgreementRejectsMovesFromRetired()
    {
        var item = SentimentAgreement.NewSentimentAgreement("code-800", "name-912");
        item.Status = SentimentAgreementStatus.Retired;
        Check.Fails(() => item.MoveTo(SentimentAgreementStatus.Created), "move");
    }

    public static void RunAll()
    {
        SentimentAgreementStartsInCreated();
        SentimentAgreementMovesToPublished();
        SentimentAgreementRejectsMovesFromRetired();
    }
}
