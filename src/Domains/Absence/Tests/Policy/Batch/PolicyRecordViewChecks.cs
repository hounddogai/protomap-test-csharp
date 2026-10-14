namespace Acme.Workforce.Absence.Tests.Policy.Batch;

using Acme.Workforce.Absence.Policy.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PolicyRecordViewChecks
{
    public static void PolicyRecordViewShowsTheStatusInCapitals()
    {
        var item = PolicyRecord.NewPolicyRecord("code-846", "label-977");
        var view = PolicyRecordView.ToPolicyRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyRecordView.ToPolicyRecordViews(new List<PolicyRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyRecordViewShowsTheStatusInCapitals();
    }
}
