namespace Acme.Workforce.Contractors.Tests.Engagement.Adjustment;

using Acme.Workforce.Contractors.Engagement.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class EngagementScoreStoreChecks
{
    public static void MemoryEngagementScoreStoreFindsWhatItSaved()
    {
        var store = EngagementScoreStore.NewMemoryEngagementScoreStore();
        var item = EngagementScore.NewEngagementScore("code-742", "label-275");
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
        MemoryEngagementScoreStoreFindsWhatItSaved();
    }
}
