namespace Acme.Workforce.Compliance.Tests.PolicyAck.Cycle;

using Acme.Workforce.Compliance.PolicyAck.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ManualPolicyAckGroupViewChecks
{
    public static void ManualPolicyAckGroupViewShowsTheStatusInCapitals()
    {
        var item = ManualPolicyAckGroup.NewManualPolicyAckGroup("code-674", "name-237");
        var view = ManualPolicyAckGroupView.ToManualPolicyAckGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualPolicyAckGroupView.ToManualPolicyAckGroupViews(new List<ManualPolicyAckGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualPolicyAckGroupViewShowsTheStatusInCapitals();
    }
}
