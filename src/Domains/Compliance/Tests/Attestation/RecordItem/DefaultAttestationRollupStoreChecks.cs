namespace Acme.Workforce.Compliance.Tests.Attestation.RecordItem;

using Acme.Workforce.Compliance.Attestation.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class DefaultAttestationRollupStoreChecks
{
    public static void MemoryDefaultAttestationRollupStoreFindsWhatItSaved()
    {
        var store = DefaultAttestationRollupStore.NewMemoryDefaultAttestationRollupStore();
        var item = DefaultAttestationRollup.NewDefaultAttestationRollup("code-914", "label-874");
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
        MemoryDefaultAttestationRollupStoreFindsWhatItSaved();
    }
}
