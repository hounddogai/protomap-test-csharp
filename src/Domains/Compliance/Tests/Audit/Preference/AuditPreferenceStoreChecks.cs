namespace Acme.Workforce.Compliance.Tests.Audit.Preference;

using Acme.Workforce.Compliance.Audit.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class AuditPreferenceStoreChecks
{
    public static void MemoryAuditPreferenceStoreFindsWhatItSaved()
    {
        var store = AuditPreferenceStore.NewMemoryAuditPreferenceStore();
        var item = AuditPreference.NewAuditPreference("code-709", "label-617");
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
        MemoryAuditPreferenceStoreFindsWhatItSaved();
    }
}
