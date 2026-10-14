namespace Acme.Workforce.Compliance.Tests.TrainingDue.Ledger;

using Acme.Workforce.Compliance.TrainingDue.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RollingTrainingDueLedgerViewChecks
{
    public static void RollingTrainingDueLedgerViewShowsTheStatusInCapitals()
    {
        var item = RollingTrainingDueLedger.NewRollingTrainingDueLedger("code-937", "label-103");
        var view = RollingTrainingDueLedgerView.ToRollingTrainingDueLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingTrainingDueLedgerView.ToRollingTrainingDueLedgerViews(new List<RollingTrainingDueLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingTrainingDueLedgerViewShowsTheStatusInCapitals();
    }
}
