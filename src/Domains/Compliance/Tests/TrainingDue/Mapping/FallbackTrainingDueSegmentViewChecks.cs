namespace Acme.Workforce.Compliance.Tests.TrainingDue.Mapping;

using Acme.Workforce.Compliance.TrainingDue.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class FallbackTrainingDueSegmentViewChecks
{
    public static void FallbackTrainingDueSegmentViewShowsTheStatusInCapitals()
    {
        var item = FallbackTrainingDueSegment.NewFallbackTrainingDueSegment("code-589", "label-410");
        var view = FallbackTrainingDueSegmentView.ToFallbackTrainingDueSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackTrainingDueSegmentView.ToFallbackTrainingDueSegmentViews(new List<FallbackTrainingDueSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackTrainingDueSegmentViewShowsTheStatusInCapitals();
    }
}
