namespace Acme.Workforce.Engagement.Tests.Feedback.Notice;

using Acme.Workforce.Engagement.Feedback.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TieredFeedbackPreferenceChecks
{
    public static void TieredFeedbackPreferenceStartsInOpen()
    {
        var item = TieredFeedbackPreference.NewTieredFeedbackPreference("code-430", "name-420");
        Check.Equal(item.Status, TieredFeedbackPreferenceStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredFeedbackPreferenceMovesToInReview()
    {
        var item = TieredFeedbackPreference.NewTieredFeedbackPreference("code-430", "name-420");
        try
        {
            item.MoveTo(TieredFeedbackPreferenceStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredFeedbackPreferenceStatus.InReview, "status");
    }

    public static void TieredFeedbackPreferenceRejectsMovesFromArchived()
    {
        var item = TieredFeedbackPreference.NewTieredFeedbackPreference("code-430", "name-420");
        item.Status = TieredFeedbackPreferenceStatus.Archived;
        Check.Fails(() => item.MoveTo(TieredFeedbackPreferenceStatus.Open), "move");
    }

    public static void RunAll()
    {
        TieredFeedbackPreferenceStartsInOpen();
        TieredFeedbackPreferenceMovesToInReview();
        TieredFeedbackPreferenceRejectsMovesFromArchived();
    }
}
