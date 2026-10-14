namespace Acme.Workforce.Compliance.Tests.TrainingDue.Batch;

using Acme.Workforce.Compliance.TrainingDue.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class TrainingDueQuotaViewChecks
{
    public static void TrainingDueQuotaViewShowsTheStatusInCapitals()
    {
        var item = TrainingDueQuota.NewTrainingDueQuota("code-867", "label-76");
        var view = TrainingDueQuotaView.ToTrainingDueQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TrainingDueQuotaView.ToTrainingDueQuotaViews(new List<TrainingDueQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TrainingDueQuotaViewShowsTheStatusInCapitals();
    }
}
