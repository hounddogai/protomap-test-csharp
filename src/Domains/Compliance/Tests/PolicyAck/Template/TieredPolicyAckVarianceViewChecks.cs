namespace Acme.Workforce.Compliance.Tests.PolicyAck.Template;

using Acme.Workforce.Compliance.PolicyAck.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class TieredPolicyAckVarianceViewChecks
{
    public static void TieredPolicyAckVarianceViewShowsTheStatusInCapitals()
    {
        var item = TieredPolicyAckVariance.NewTieredPolicyAckVariance("code-911", "label-24");
        var view = TieredPolicyAckVarianceView.ToTieredPolicyAckVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredPolicyAckVarianceView.ToTieredPolicyAckVarianceViews(new List<TieredPolicyAckVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredPolicyAckVarianceViewShowsTheStatusInCapitals();
    }
}
