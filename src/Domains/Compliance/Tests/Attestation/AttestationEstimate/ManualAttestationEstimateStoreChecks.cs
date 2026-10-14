namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationEstimate;

using Acme.Workforce.Compliance.Attestation.AttestationEstimate;

/// <summary>Checks of attestation estimate code, run by the project's runner.</summary>
public static class ManualAttestationEstimateStoreChecks
{
    public static void MemoryManualAttestationEstimateStoreFindsWhatItSaved()
    {
        var store = ManualAttestationEstimateStore.NewMemoryManualAttestationEstimateStore();
        var item = ManualAttestationEstimate.NewManualAttestationEstimate("code-142", "label-981");
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
        MemoryManualAttestationEstimateStoreFindsWhatItSaved();
    }
}
