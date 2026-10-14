namespace Acme.Workforce.Compliance.Tests.PolicyAck.Reservation;

using Acme.Workforce.Compliance.PolicyAck.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class TieredPolicyAckOverrideViewChecks
{
    public static void TieredPolicyAckOverrideViewShowsTheStatusInCapitals()
    {
        var item = TieredPolicyAckOverride.NewTieredPolicyAckOverride("code-69", "label-144");
        var view = TieredPolicyAckOverrideView.ToTieredPolicyAckOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredPolicyAckOverrideView.ToTieredPolicyAckOverrideViews(new List<TieredPolicyAckOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredPolicyAckOverrideViewShowsTheStatusInCapitals();
    }
}
