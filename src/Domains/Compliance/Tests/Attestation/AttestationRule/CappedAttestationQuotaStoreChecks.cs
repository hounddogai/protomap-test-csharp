namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationRule;

using Acme.Workforce.Compliance.Attestation.AttestationRule;

/// <summary>Checks of attestation rule code, run by the project's runner.</summary>
public static class CappedAttestationQuotaStoreChecks
{
    public static void MemoryCappedAttestationQuotaStoreFindsWhatItSaved()
    {
        var store = CappedAttestationQuotaStore.NewMemoryCappedAttestationQuotaStore();
        var item = CappedAttestationQuota.NewCappedAttestationQuota("code-760", "label-986");
        store.Save(item);
        if (store.Find(item.Code) is not { } found)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(found.Code, item.Code, "code");
        Check.Equal(store.List().Count, 1, "count");
    }

    public static void RunAll()
    {
        MemoryCappedAttestationQuotaStoreFindsWhatItSaved();
    }
}
