namespace Acme.Workforce.Engagement.Tests.Feedback.Proposal;

using Acme.Workforce.Engagement.Feedback.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class FeedbackBaselineChecks
{
    public static void FeedbackBaselineStartsInCreated()
    {
        var item = FeedbackBaseline.NewFeedbackBaseline("code-670", "title-820");
        Check.Equal(item.Status, FeedbackBaselineStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackBaselineMovesToValidated()
    {
        var item = FeedbackBaseline.NewFeedbackBaseline("code-670", "title-820");
        try
        {
            item.MoveTo(FeedbackBaselineStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackBaselineStatus.Validated, "status");
    }

    public static void FeedbackBaselineRejectsMovesFromRetired()
    {
        var item = FeedbackBaseline.NewFeedbackBaseline("code-670", "title-820");
        item.Status = FeedbackBaselineStatus.Retired;
        Check.Fails(() => item.MoveTo(FeedbackBaselineStatus.Created), "move");
    }

    public static void RunAll()
    {
        FeedbackBaselineStartsInCreated();
        FeedbackBaselineMovesToValidated();
        FeedbackBaselineRejectsMovesFromRetired();
    }
}
