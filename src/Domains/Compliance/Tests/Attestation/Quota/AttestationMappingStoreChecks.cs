namespace Acme.Workforce.Compliance.Tests.Attestation.Quota;

using Acme.Workforce.Compliance.Attestation.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AttestationMappingStoreChecks
{
    public static void MemoryAttestationMappingStoreFindsWhatItSaved()
    {
        var store = AttestationMappingStore.NewMemoryAttestationMappingStore();
        var item = AttestationMapping.NewAttestationMapping("code-425", "label-236");
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
        MemoryAttestationMappingStoreFindsWhatItSaved();
    }
}
