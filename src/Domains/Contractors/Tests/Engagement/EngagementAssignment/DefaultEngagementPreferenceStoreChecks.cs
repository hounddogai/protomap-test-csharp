namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementAssignment;

using Acme.Workforce.Contractors.Engagement.EngagementAssignment;

/// <summary>Checks of engagement assignment code, run by the project's runner.</summary>
public static class DefaultEngagementPreferenceStoreChecks
{
    public static void MemoryDefaultEngagementPreferenceStoreFindsWhatItSaved()
    {
        var store = DefaultEngagementPreferenceStore.NewMemoryDefaultEngagementPreferenceStore();
        var item = DefaultEngagementPreference.NewDefaultEngagementPreference("code-882", "name-335");
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
        MemoryDefaultEngagementPreferenceStoreFindsWhatItSaved();
    }
}
