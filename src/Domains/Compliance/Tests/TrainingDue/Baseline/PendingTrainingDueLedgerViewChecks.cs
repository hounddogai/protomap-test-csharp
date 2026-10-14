namespace Acme.Workforce.Compliance.Tests.TrainingDue.Baseline;

using Acme.Workforce.Compliance.TrainingDue.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PendingTrainingDueLedgerViewChecks
{
    public static void PendingTrainingDueLedgerViewShowsTheStatusInCapitals()
    {
        var item = PendingTrainingDueLedger.NewPendingTrainingDueLedger("code-162", "name-36");
        var view = PendingTrainingDueLedgerView.ToPendingTrainingDueLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingTrainingDueLedgerView.ToPendingTrainingDueLedgerViews(new List<PendingTrainingDueLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingTrainingDueLedgerViewShowsTheStatusInCapitals();
    }
}
