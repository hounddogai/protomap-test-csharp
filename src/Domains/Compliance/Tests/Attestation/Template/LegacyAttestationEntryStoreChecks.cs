namespace Acme.Workforce.Compliance.Tests.Attestation.Template;

using Acme.Workforce.Compliance.Attestation.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LegacyAttestationEntryStoreChecks
{
    public static void MemoryLegacyAttestationEntryStoreFindsWhatItSaved()
    {
        var store = LegacyAttestationEntryStore.NewMemoryLegacyAttestationEntryStore();
        var item = LegacyAttestationEntry.NewLegacyAttestationEntry("code-377", "label-36");
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
        MemoryLegacyAttestationEntryStoreFindsWhatItSaved();
    }
}
