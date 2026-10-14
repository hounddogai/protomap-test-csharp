namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationRule;

using Acme.Workforce.Compliance.Attestation.AttestationRule;

/// <summary>Checks of attestation rule code, run by the project's runner.</summary>
public static class CappedAttestationQuotaChecks
{
    public static void CappedAttestationQuotaStartsInCreated()
    {
        var item = CappedAttestationQuota.NewCappedAttestationQuota("code-760", "label-986");
        Check.Equal(item.Status, CappedAttestationQuotaStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedAttestationQuotaMovesToPublished()
    {
        var item = CappedAttestationQuota.NewCappedAttestationQuota("code-760", "label-986");
        try
        {
            item.MoveTo(CappedAttestationQuotaStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedAttestationQuotaStatus.Published, "status");
    }

    public static void CappedAttestationQuotaRejectsMovesFromRetired()
    {
        var item = CappedAttestationQuota.NewCappedAttestationQuota("code-760", "label-986");
        item.Status = CappedAttestationQuotaStatus.Retired;
        Check.Fails(() => item.MoveTo(CappedAttestationQuotaStatus.Created), "move");
    }

    public static void RunAll()
    {
        CappedAttestationQuotaStartsInCreated();
        CappedAttestationQuotaMovesToPublished();
        CappedAttestationQuotaRejectsMovesFromRetired();
    }
}
