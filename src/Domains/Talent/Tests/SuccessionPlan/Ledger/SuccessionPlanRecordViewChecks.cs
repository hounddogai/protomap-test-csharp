namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Ledger;

using Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SuccessionPlanRecordViewChecks
{
    public static void SuccessionPlanRecordViewShowsTheStatusInCapitals()
    {
        var item = SuccessionPlanRecord.NewSuccessionPlanRecord("code-664", "name-570");
        var view = SuccessionPlanRecordView.ToSuccessionPlanRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SuccessionPlanRecordView.ToSuccessionPlanRecordViews(new List<SuccessionPlanRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SuccessionPlanRecordViewShowsTheStatusInCapitals();
    }
}
