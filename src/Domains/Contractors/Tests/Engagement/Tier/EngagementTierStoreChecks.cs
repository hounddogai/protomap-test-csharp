namespace Acme.Workforce.Contractors.Tests.Engagement.Tier;

using Acme.Workforce.Contractors.Engagement.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class EngagementTierStoreChecks
{
    public static void MemoryEngagementTierStoreFindsWhatItSaved()
    {
        var store = EngagementTierStore.NewMemoryEngagementTierStore();
        var item = EngagementTier.NewEngagementTier("code-516", "label-748");
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
        MemoryEngagementTierStoreFindsWhatItSaved();
    }
}
