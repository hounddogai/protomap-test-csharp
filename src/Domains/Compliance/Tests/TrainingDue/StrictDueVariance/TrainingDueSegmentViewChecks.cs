namespace Acme.Workforce.Compliance.Tests.TrainingDue.StrictDueVariance;

using Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

/// <summary>Checks of strict due variance code, run by the project's runner.</summary>
public static class TrainingDueSegmentViewChecks
{
    public static void TrainingDueSegmentViewShowsTheStatusInCapitals()
    {
        var item = TrainingDueSegment.NewTrainingDueSegment("code-304", "name-297");
        var view = TrainingDueSegmentView.ToTrainingDueSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TrainingDueSegmentView.ToTrainingDueSegmentViews(new List<TrainingDueSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TrainingDueSegmentViewShowsTheStatusInCapitals();
    }
}
