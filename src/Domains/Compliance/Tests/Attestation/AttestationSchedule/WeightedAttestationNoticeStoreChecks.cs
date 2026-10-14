namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationSchedule;

using Acme.Workforce.Compliance.Attestation.AttestationSchedule;

/// <summary>Checks of attestation schedule code, run by the project's runner.</summary>
public static class WeightedAttestationNoticeStoreChecks
{
    public static void MemoryWeightedAttestationNoticeStoreFindsWhatItSaved()
    {
        var store = WeightedAttestationNoticeStore.NewMemoryWeightedAttestationNoticeStore();
        var item = WeightedAttestationNotice.NewWeightedAttestationNotice("code-815", "name-422");
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
        MemoryWeightedAttestationNoticeStoreFindsWhatItSaved();
    }
}
