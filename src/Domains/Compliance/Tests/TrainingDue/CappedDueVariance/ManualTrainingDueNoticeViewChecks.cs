namespace Acme.Workforce.Compliance.Tests.TrainingDue.CappedDueVariance;

using Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

/// <summary>Checks of capped due variance code, run by the project's runner.</summary>
public static class ManualTrainingDueNoticeViewChecks
{
    public static void ManualTrainingDueNoticeViewShowsTheStatusInCapitals()
    {
        var item = ManualTrainingDueNotice.NewManualTrainingDueNotice("code-776", "name-901");
        var view = ManualTrainingDueNoticeView.ToManualTrainingDueNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualTrainingDueNoticeView.ToManualTrainingDueNoticeViews(new List<ManualTrainingDueNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualTrainingDueNoticeViewShowsTheStatusInCapitals();
    }
}
