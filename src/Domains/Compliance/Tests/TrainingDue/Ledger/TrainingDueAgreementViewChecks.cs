namespace Acme.Workforce.Compliance.Tests.TrainingDue.Ledger;

using Acme.Workforce.Compliance.TrainingDue.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class TrainingDueAgreementViewChecks
{
    public static void TrainingDueAgreementViewShowsTheStatusInCapitals()
    {
        var item = TrainingDueAgreement.NewTrainingDueAgreement("code-997", "label-182");
        var view = TrainingDueAgreementView.ToTrainingDueAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TrainingDueAgreementView.ToTrainingDueAgreementViews(new List<TrainingDueAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TrainingDueAgreementViewShowsTheStatusInCapitals();
    }
}
