namespace Acme.Workforce.Compliance.Tests.PolicyAck.Exception;

using Acme.Workforce.Compliance.PolicyAck.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PolicyAckManifestViewChecks
{
    public static void PolicyAckManifestViewShowsTheStatusInCapitals()
    {
        var item = PolicyAckManifest.NewPolicyAckManifest("code-943", "label-97");
        var view = PolicyAckManifestView.ToPolicyAckManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyAckManifestView.ToPolicyAckManifestViews(new List<PolicyAckManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyAckManifestViewShowsTheStatusInCapitals();
    }
}
