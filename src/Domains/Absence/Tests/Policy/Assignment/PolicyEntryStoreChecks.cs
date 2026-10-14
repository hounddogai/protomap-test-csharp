namespace Acme.Workforce.Absence.Tests.Policy.Assignment;

using Acme.Workforce.Absence.Policy.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class PolicyEntryStoreChecks
{
    public static void MemoryPolicyEntryStoreFindsWhatItSaved()
    {
        var store = PolicyEntryStore.NewMemoryPolicyEntryStore();
        var item = PolicyEntry.NewPolicyEntry("code-103", "name-270");
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
        MemoryPolicyEntryStoreFindsWhatItSaved();
    }
}
