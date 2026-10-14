namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckCycle;

using Acme.Workforce.Compliance.PolicyAck.AckCycle;

/// <summary>Checks of ack cycle code, run by the project's runner.</summary>
public static class CappedPolicyAckRollupViewChecks
{
    public static void CappedPolicyAckRollupViewShowsTheStatusInCapitals()
    {
        var item = CappedPolicyAckRollup.NewCappedPolicyAckRollup("code-816", "title-300");
        var view = CappedPolicyAckRollupView.ToCappedPolicyAckRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedPolicyAckRollupView.ToCappedPolicyAckRollupViews(new List<CappedPolicyAckRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedPolicyAckRollupViewShowsTheStatusInCapitals();
    }
}
