namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentManifest;

using Acme.Workforce.Engagement.Sentiment.SentimentManifest;

/// <summary>Checks of sentiment manifest code, run by the project's runner.</summary>
public static class SentimentAssignmentChecks
{
    public static void SentimentAssignmentStartsInCreated()
    {
        var item = SentimentAssignment.NewSentimentAssignment("code-830", "title-394");
        Check.Equal(item.Status, SentimentAssignmentStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentAssignmentMovesToPublished()
    {
        var item = SentimentAssignment.NewSentimentAssignment("code-830", "title-394");
        try
        {
            item.MoveTo(SentimentAssignmentStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentAssignmentStatus.Published, "status");
    }

    public static void SentimentAssignmentRejectsMovesFromRetired()
    {
        var item = SentimentAssignment.NewSentimentAssignment("code-830", "title-394");
        item.Status = SentimentAssignmentStatus.Retired;
        Check.Fails(() => item.MoveTo(SentimentAssignmentStatus.Created), "move");
    }

    public static void RunAll()
    {
        SentimentAssignmentStartsInCreated();
        SentimentAssignmentMovesToPublished();
        SentimentAssignmentRejectsMovesFromRetired();
    }
}
