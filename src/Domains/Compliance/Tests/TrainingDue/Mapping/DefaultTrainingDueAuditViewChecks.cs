namespace Acme.Workforce.Compliance.Tests.TrainingDue.Mapping;

using Acme.Workforce.Compliance.TrainingDue.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DefaultTrainingDueAuditViewChecks
{
    public static void DefaultTrainingDueAuditViewShowsTheStatusInCapitals()
    {
        var item = DefaultTrainingDueAudit.NewDefaultTrainingDueAudit("code-558", "title-74");
        var view = DefaultTrainingDueAuditView.ToDefaultTrainingDueAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultTrainingDueAuditView.ToDefaultTrainingDueAuditViews(new List<DefaultTrainingDueAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultTrainingDueAuditViewShowsTheStatusInCapitals();
    }
}
