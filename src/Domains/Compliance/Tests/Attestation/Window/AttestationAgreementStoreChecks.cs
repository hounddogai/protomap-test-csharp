namespace Acme.Workforce.Compliance.Tests.Attestation.Window;

using Acme.Workforce.Compliance.Attestation.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class AttestationAgreementStoreChecks
{
    public static void MemoryAttestationAgreementStoreFindsWhatItSaved()
    {
        var store = AttestationAgreementStore.NewMemoryAttestationAgreementStore();
        var item = AttestationAgreement.NewAttestationAgreement("code-417", "label-6");
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
        MemoryAttestationAgreementStoreFindsWhatItSaved();
    }
}
