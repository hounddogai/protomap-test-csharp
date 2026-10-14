namespace Acme.Workforce.Compliance.Tests.Attestation.Rollup;

using Acme.Workforce.Compliance.Attestation.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AttestationSegmentStoreChecks
{
    public static void MemoryAttestationSegmentStoreFindsWhatItSaved()
    {
        var store = AttestationSegmentStore.NewMemoryAttestationSegmentStore();
        var item = AttestationSegment.NewAttestationSegment("code-221", "name-944");
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
        MemoryAttestationSegmentStoreFindsWhatItSaved();
    }
}
