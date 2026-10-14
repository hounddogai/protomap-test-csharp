namespace Acme.Workforce.Compliance.Tests.TrainingDue.Quota;

using Acme.Workforce.Compliance.TrainingDue.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class StrictTrainingDueQuotaViewChecks
{
    public static void StrictTrainingDueQuotaViewShowsTheStatusInCapitals()
    {
        var item = StrictTrainingDueQuota.NewStrictTrainingDueQuota("code-407", "label-589");
        var view = StrictTrainingDueQuotaView.ToStrictTrainingDueQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictTrainingDueQuotaView.ToStrictTrainingDueQuotaViews(new List<StrictTrainingDueQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictTrainingDueQuotaViewShowsTheStatusInCapitals();
    }
}
