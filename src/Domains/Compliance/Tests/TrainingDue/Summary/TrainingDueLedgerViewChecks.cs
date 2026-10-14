namespace Acme.Workforce.Compliance.Tests.TrainingDue.Summary;

using Acme.Workforce.Compliance.TrainingDue.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class TrainingDueLedgerViewChecks
{
    public static void TrainingDueLedgerViewShowsTheStatusInCapitals()
    {
        var item = TrainingDueLedger.NewTrainingDueLedger("code-371", "name-830");
        var view = TrainingDueLedgerView.ToTrainingDueLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TrainingDueLedgerView.ToTrainingDueLedgerViews(new List<TrainingDueLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TrainingDueLedgerViewShowsTheStatusInCapitals();
    }
}
