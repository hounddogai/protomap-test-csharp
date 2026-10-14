namespace Acme.Workforce.Compliance.Tests.Attestation.Quota;

using Acme.Workforce.Compliance.Attestation.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AttestationMappingViewChecks
{
    public static void AttestationMappingViewShowsTheStatusInCapitals()
    {
        var item = AttestationMapping.NewAttestationMapping("code-425", "label-236");
        var view = AttestationMappingView.ToAttestationMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AttestationMappingView.ToAttestationMappingViews(new List<AttestationMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AttestationMappingViewShowsTheStatusInCapitals();
    }
}
