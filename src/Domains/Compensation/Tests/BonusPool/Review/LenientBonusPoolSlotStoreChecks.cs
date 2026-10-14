namespace Acme.Workforce.Compensation.Tests.BonusPool.Review;

using Acme.Workforce.Compensation.BonusPool.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LenientBonusPoolSlotStoreChecks
{
    public static void MemoryLenientBonusPoolSlotStoreFindsWhatItSaved()
    {
        var store = LenientBonusPoolSlotStore.NewMemoryLenientBonusPoolSlotStore();
        var item = LenientBonusPoolSlot.NewLenientBonusPoolSlot("code-203", "title-221");
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
        MemoryLenientBonusPoolSlotStoreFindsWhatItSaved();
    }
}
