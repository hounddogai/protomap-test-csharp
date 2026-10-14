namespace Acme.Workforce.Absence.Tests.Policy.WeightedPolicyPreference;

using Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

/// <summary>Checks of weighted policy preference code, run by the project's runner.</summary>
public static class StrictPolicyEntryStoreChecks
{
    public static void MemoryStrictPolicyEntryStoreFindsWhatItSaved()
    {
        var store = StrictPolicyEntryStore.NewMemoryStrictPolicyEntryStore();
        var item = StrictPolicyEntry.NewStrictPolicyEntry("code-352", "name-282");
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
        MemoryStrictPolicyEntryStoreFindsWhatItSaved();
    }
}
