namespace Acme.Workforce.Compensation.Tests.BonusPool.Batch;

using Acme.Workforce.Compensation.BonusPool.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BonusPoolRequestStoreChecks
{
    public static void MemoryBonusPoolRequestStoreFindsWhatItSaved()
    {
        var store = BonusPoolRequestStore.NewMemoryBonusPoolRequestStore();
        var item = BonusPoolRequest.NewBonusPoolRequest("code-557", "title-986");
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
        MemoryBonusPoolRequestStoreFindsWhatItSaved();
    }
}
