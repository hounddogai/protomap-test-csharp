namespace Acme.Workforce.Compliance.Tests.PolicyAck.Estimate;

using Acme.Workforce.Compliance.PolicyAck.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PrimaryPolicyAckPlanViewChecks
{
    public static void PrimaryPolicyAckPlanViewShowsTheStatusInCapitals()
    {
        var item = PrimaryPolicyAckPlan.NewPrimaryPolicyAckPlan("code-392", "label-505");
        var view = PrimaryPolicyAckPlanView.ToPrimaryPolicyAckPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryPolicyAckPlanView.ToPrimaryPolicyAckPlanViews(new List<PrimaryPolicyAckPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryPolicyAckPlanViewShowsTheStatusInCapitals();
    }
}
