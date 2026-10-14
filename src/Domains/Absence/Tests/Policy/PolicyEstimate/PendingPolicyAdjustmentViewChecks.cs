namespace Acme.Workforce.Absence.Tests.Policy.PolicyEstimate;

using Acme.Workforce.Absence.Policy.PolicyEstimate;

/// <summary>Checks of policy estimate code, run by the project's runner.</summary>
public static class PendingPolicyAdjustmentViewChecks
{
    public static void PendingPolicyAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = PendingPolicyAdjustment.NewPendingPolicyAdjustment("code-482", "title-215");
        var view = PendingPolicyAdjustmentView.ToPendingPolicyAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingPolicyAdjustmentView.ToPendingPolicyAdjustmentViews(new List<PendingPolicyAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingPolicyAdjustmentViewShowsTheStatusInCapitals();
    }
}
