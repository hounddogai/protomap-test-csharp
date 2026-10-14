namespace Acme.Workforce.Absence.Tests.Policy.PolicyPreference;

using Acme.Workforce.Absence.Policy.PolicyPreference;

/// <summary>Checks of policy preference code, run by the project's runner.</summary>
public static class StrictPolicyPreferenceStoreChecks
{
    public static void MemoryStrictPolicyPreferenceStoreFindsWhatItSaved()
    {
        var store = StrictPolicyPreferenceStore.NewMemoryStrictPolicyPreferenceStore();
        var item = StrictPolicyPreference.NewStrictPolicyPreference("code-691", "title-468");
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
        MemoryStrictPolicyPreferenceStoreFindsWhatItSaved();
    }
}
