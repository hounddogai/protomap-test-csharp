namespace Acme.Workforce.Contractors.Tests.Engagement.Assignment;

using Acme.Workforce.Contractors.Engagement.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class EngagementPreferenceStoreChecks
{
    public static void MemoryEngagementPreferenceStoreFindsWhatItSaved()
    {
        var store = EngagementPreferenceStore.NewMemoryEngagementPreferenceStore();
        var item = EngagementPreference.NewEngagementPreference("code-908", "name-996");
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
        MemoryEngagementPreferenceStoreFindsWhatItSaved();
    }
}
