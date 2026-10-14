namespace Acme.Workforce.Compliance.Tests.PolicyAck.Claim;

using Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class GlobalPolicyAckBundleViewChecks
{
    public static void GlobalPolicyAckBundleViewShowsTheStatusInCapitals()
    {
        var item = GlobalPolicyAckBundle.NewGlobalPolicyAckBundle("code-987", "title-677");
        var view = GlobalPolicyAckBundleView.ToGlobalPolicyAckBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalPolicyAckBundleView.ToGlobalPolicyAckBundleViews(new List<GlobalPolicyAckBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalPolicyAckBundleViewShowsTheStatusInCapitals();
    }
}
