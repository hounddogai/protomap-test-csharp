namespace Acme.Workforce.Compliance.Tests.Attestation.Segment;

using Acme.Workforce.Compliance.Attestation.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class PendingAttestationSegmentStoreChecks
{
    public static void MemoryPendingAttestationSegmentStoreFindsWhatItSaved()
    {
        var store = PendingAttestationSegmentStore.NewMemoryPendingAttestationSegmentStore();
        var item = PendingAttestationSegment.NewPendingAttestationSegment("code-294", "title-302");
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
        MemoryPendingAttestationSegmentStoreFindsWhatItSaved();
    }
}
