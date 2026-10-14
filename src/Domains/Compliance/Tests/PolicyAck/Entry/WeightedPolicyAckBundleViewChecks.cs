namespace Acme.Workforce.Compliance.Tests.PolicyAck.Entry;

using Acme.Workforce.Compliance.PolicyAck.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class WeightedPolicyAckBundleViewChecks
{
    public static void WeightedPolicyAckBundleViewShowsTheStatusInCapitals()
    {
        var item = WeightedPolicyAckBundle.NewWeightedPolicyAckBundle("code-305", "label-926");
        var view = WeightedPolicyAckBundleView.ToWeightedPolicyAckBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedPolicyAckBundleView.ToWeightedPolicyAckBundleViews(new List<WeightedPolicyAckBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedPolicyAckBundleViewShowsTheStatusInCapitals();
    }
}
