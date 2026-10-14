namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationRule;

using Acme.Workforce.Compliance.Attestation.AttestationRule;

/// <summary>Checks of attestation rule code, run by the project's runner.</summary>
public static class CappedAttestationQuotaViewChecks
{
    public static void CappedAttestationQuotaViewShowsTheStatusInCapitals()
    {
        var item = CappedAttestationQuota.NewCappedAttestationQuota("code-760", "label-986");
        var view = CappedAttestationQuotaView.ToCappedAttestationQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedAttestationQuotaView.ToCappedAttestationQuotaViews(new List<CappedAttestationQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedAttestationQuotaViewShowsTheStatusInCapitals();
    }
}
