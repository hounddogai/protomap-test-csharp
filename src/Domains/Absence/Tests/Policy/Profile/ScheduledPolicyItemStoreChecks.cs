namespace Acme.Workforce.Absence.Tests.Policy.Profile;

using Acme.Workforce.Absence.Policy.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class ScheduledPolicyItemStoreChecks
{
    public static void MemoryScheduledPolicyItemStoreFindsWhatItSaved()
    {
        var store = ScheduledPolicyItemStore.NewMemoryScheduledPolicyItemStore();
        var item = ScheduledPolicyItem.NewScheduledPolicyItem("code-967", "title-667");
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
        MemoryScheduledPolicyItemStoreFindsWhatItSaved();
    }
}
