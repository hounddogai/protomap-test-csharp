namespace Acme.Workforce.Compliance.Tests.Attestation.Policy;

using Acme.Workforce.Compliance.Attestation.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AttestationPolicyStoreChecks
{
    public static void MemoryAttestationPolicyStoreFindsWhatItSaved()
    {
        var store = AttestationPolicyStore.NewMemoryAttestationPolicyStore();
        var item = AttestationPolicy.NewAttestationPolicy("code-435", "name-281");
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
        MemoryAttestationPolicyStoreFindsWhatItSaved();
    }
}
