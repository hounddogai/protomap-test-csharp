namespace Acme.Workforce.Engagement.Tests.Feedback.Audit;

using Acme.Workforce.Engagement.Feedback.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class AdaptiveFeedbackPreferenceChecks
{
    public static void AdaptiveFeedbackPreferenceStartsInCreated()
    {
        var item = AdaptiveFeedbackPreference.NewAdaptiveFeedbackPreference("code-850", "name-39");
        Check.Equal(item.Status, AdaptiveFeedbackPreferenceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveFeedbackPreferenceMovesToPublished()
    {
        var item = AdaptiveFeedbackPreference.NewAdaptiveFeedbackPreference("code-850", "name-39");
        try
        {
            item.MoveTo(AdaptiveFeedbackPreferenceStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveFeedbackPreferenceStatus.Published, "status");
    }

    public static void AdaptiveFeedbackPreferenceRejectsMovesFromRetired()
    {
        var item = AdaptiveFeedbackPreference.NewAdaptiveFeedbackPreference("code-850", "name-39");
        item.Status = AdaptiveFeedbackPreferenceStatus.Retired;
        Check.Fails(() => item.MoveTo(AdaptiveFeedbackPreferenceStatus.Created), "move");
    }

    public static void RunAll()
    {
        AdaptiveFeedbackPreferenceStartsInCreated();
        AdaptiveFeedbackPreferenceMovesToPublished();
        AdaptiveFeedbackPreferenceRejectsMovesFromRetired();
    }
}
