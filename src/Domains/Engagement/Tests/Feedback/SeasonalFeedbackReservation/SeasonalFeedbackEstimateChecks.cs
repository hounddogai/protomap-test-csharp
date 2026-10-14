namespace Acme.Workforce.Engagement.Tests.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

/// <summary>Checks of seasonal feedback reservation code, run by the project's runner.</summary>
public static class SeasonalFeedbackEstimateChecks
{
    public static void SeasonalFeedbackEstimateStartsInPlanned()
    {
        var item = SeasonalFeedbackEstimate.NewSeasonalFeedbackEstimate("code-815", "label-859");
        Check.Equal(item.Status, SeasonalFeedbackEstimateStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalFeedbackEstimateMovesToCompleted()
    {
        var item = SeasonalFeedbackEstimate.NewSeasonalFeedbackEstimate("code-815", "label-859");
        try
        {
            item.MoveTo(SeasonalFeedbackEstimateStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalFeedbackEstimateStatus.Completed, "status");
    }

    public static void SeasonalFeedbackEstimateRejectsMovesFromCancelled()
    {
        var item = SeasonalFeedbackEstimate.NewSeasonalFeedbackEstimate("code-815", "label-859");
        item.Status = SeasonalFeedbackEstimateStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SeasonalFeedbackEstimateStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SeasonalFeedbackEstimateStartsInPlanned();
        SeasonalFeedbackEstimateMovesToCompleted();
        SeasonalFeedbackEstimateRejectsMovesFromCancelled();
    }
}
