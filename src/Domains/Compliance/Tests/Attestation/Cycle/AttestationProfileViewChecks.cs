namespace Acme.Workforce.Compliance.Tests.Attestation.Cycle;

using Acme.Workforce.Compliance.Attestation.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AttestationProfileViewChecks
{
    public static void AttestationProfileViewShowsTheStatusInCapitals()
    {
        var item = AttestationProfile.NewAttestationProfile("code-296", "label-743");
        var view = AttestationProfileView.ToAttestationProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AttestationProfileView.ToAttestationProfileViews(new List<AttestationProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AttestationProfileViewShowsTheStatusInCapitals();
    }
}
