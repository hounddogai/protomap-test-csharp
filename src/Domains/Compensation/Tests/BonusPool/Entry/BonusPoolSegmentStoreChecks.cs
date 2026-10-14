namespace Acme.Workforce.Compensation.Tests.BonusPool.Entry;

using Acme.Workforce.Compensation.BonusPool.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BonusPoolSegmentStoreChecks
{
    public static void MemoryBonusPoolSegmentStoreFindsWhatItSaved()
    {
        var store = BonusPoolSegmentStore.NewMemoryBonusPoolSegmentStore();
        var item = BonusPoolSegment.NewBonusPoolSegment("code-848", "label-929");
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
        MemoryBonusPoolSegmentStoreFindsWhatItSaved();
    }
}
