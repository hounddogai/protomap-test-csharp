namespace Acme.Workforce.Compliance.Tests.TrainingDue.Plan;

using Acme.Workforce.Compliance.TrainingDue.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class TrainingDueReviewViewChecks
{
    public static void TrainingDueReviewViewShowsTheStatusInCapitals()
    {
        var item = TrainingDueReview.NewTrainingDueReview("code-434", "label-533");
        var view = TrainingDueReviewView.ToTrainingDueReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TrainingDueReviewView.ToTrainingDueReviewViews(new List<TrainingDueReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TrainingDueReviewViewShowsTheStatusInCapitals();
    }
}
