namespace Acme.Workforce.Compliance.Tests.TrainingDue.Ledger;

using Acme.Workforce.Compliance.TrainingDue.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ScheduledTrainingDueAuditViewChecks
{
    public static void ScheduledTrainingDueAuditViewShowsTheStatusInCapitals()
    {
        var item = ScheduledTrainingDueAudit.NewScheduledTrainingDueAudit("code-827", "name-935");
        var view = ScheduledTrainingDueAuditView.ToScheduledTrainingDueAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledTrainingDueAuditView.ToScheduledTrainingDueAuditViews(new List<ScheduledTrainingDueAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledTrainingDueAuditViewShowsTheStatusInCapitals();
    }
}
