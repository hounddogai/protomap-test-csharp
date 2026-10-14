namespace Acme.Workforce.Contractors.Tests.Engagement.Target;

using Acme.Workforce.Contractors.Engagement.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class EngagementRecordStoreChecks
{
    public static void MemoryEngagementRecordStoreFindsWhatItSaved()
    {
        var store = EngagementRecordStore.NewMemoryEngagementRecordStore();
        var item = EngagementRecord.NewEngagementRecord("code-629", "title-18");
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
        MemoryEngagementRecordStoreFindsWhatItSaved();
    }
}
