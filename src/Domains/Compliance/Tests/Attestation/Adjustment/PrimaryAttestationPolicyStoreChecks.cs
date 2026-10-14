namespace Acme.Workforce.Compliance.Tests.Attestation.Adjustment;

using Acme.Workforce.Compliance.Attestation.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PrimaryAttestationPolicyStoreChecks
{
    public static void MemoryPrimaryAttestationPolicyStoreFindsWhatItSaved()
    {
        var store = PrimaryAttestationPolicyStore.NewMemoryPrimaryAttestationPolicyStore();
        var item = PrimaryAttestationPolicy.NewPrimaryAttestationPolicy("code-959", "name-898");
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
        MemoryPrimaryAttestationPolicyStoreFindsWhatItSaved();
    }
}
