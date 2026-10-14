namespace Acme.Workforce.Compliance.Tests.Attestation.Baseline;

using Acme.Workforce.Compliance.Attestation.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class GlobalAttestationBaselineViewChecks
{
    public static void GlobalAttestationBaselineViewShowsTheStatusInCapitals()
    {
        var item = GlobalAttestationBaseline.NewGlobalAttestationBaseline("code-731", "label-485");
        var view = GlobalAttestationBaselineView.ToGlobalAttestationBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalAttestationBaselineView.ToGlobalAttestationBaselineViews(new List<GlobalAttestationBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalAttestationBaselineViewShowsTheStatusInCapitals();
    }
}
