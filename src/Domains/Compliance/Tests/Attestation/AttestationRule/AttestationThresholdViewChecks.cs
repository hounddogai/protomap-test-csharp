namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationRule;

using Acme.Workforce.Compliance.Attestation.AttestationRule;

/// <summary>Checks of attestation rule code, run by the project's runner.</summary>
public static class AttestationThresholdViewChecks
{
    public static void AttestationThresholdViewShowsTheStatusInCapitals()
    {
        var item = AttestationThreshold.NewAttestationThreshold("code-972", "name-614");
        var view = AttestationThresholdView.ToAttestationThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AttestationThresholdView.ToAttestationThresholdViews(new List<AttestationThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AttestationThresholdViewShowsTheStatusInCapitals();
    }
}
