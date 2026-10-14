namespace Acme.Workforce.Contractors.Tests.Engagement.Policy;

using Acme.Workforce.Contractors.Engagement.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class LenientEngagementPolicyStoreChecks
{
    public static void MemoryLenientEngagementPolicyStoreFindsWhatItSaved()
    {
        var store = LenientEngagementPolicyStore.NewMemoryLenientEngagementPolicyStore();
        var item = LenientEngagementPolicy.NewLenientEngagementPolicy("code-332", "title-759");
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
        MemoryLenientEngagementPolicyStoreFindsWhatItSaved();
    }
}
