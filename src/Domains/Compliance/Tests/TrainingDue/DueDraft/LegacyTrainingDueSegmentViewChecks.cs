namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueDraft;

using Acme.Workforce.Compliance.TrainingDue.DueDraft;

/// <summary>Checks of due draft code, run by the project's runner.</summary>
public static class LegacyTrainingDueSegmentViewChecks
{
    public static void LegacyTrainingDueSegmentViewShowsTheStatusInCapitals()
    {
        var item = LegacyTrainingDueSegment.NewLegacyTrainingDueSegment("code-822", "label-968");
        var view = LegacyTrainingDueSegmentView.ToLegacyTrainingDueSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyTrainingDueSegmentView.ToLegacyTrainingDueSegmentViews(new List<LegacyTrainingDueSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyTrainingDueSegmentViewShowsTheStatusInCapitals();
    }
}
