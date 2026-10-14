namespace Acme.Workforce.Absence.Tests.Policy.Preference;

using Acme.Workforce.Absence.Policy.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PolicyQuotaStoreChecks
{
    public static void MemoryPolicyQuotaStoreFindsWhatItSaved()
    {
        var store = PolicyQuotaStore.NewMemoryPolicyQuotaStore();
        var item = PolicyQuota.NewPolicyQuota("code-453", "name-854");
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
        MemoryPolicyQuotaStoreFindsWhatItSaved();
    }
}
