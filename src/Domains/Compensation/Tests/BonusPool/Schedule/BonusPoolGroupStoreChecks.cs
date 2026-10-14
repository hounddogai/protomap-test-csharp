namespace Acme.Workforce.Compensation.Tests.BonusPool.Schedule;

using Acme.Workforce.Compensation.BonusPool.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class BonusPoolGroupStoreChecks
{
    public static void MemoryBonusPoolGroupStoreFindsWhatItSaved()
    {
        var store = BonusPoolGroupStore.NewMemoryBonusPoolGroupStore();
        var item = BonusPoolGroup.NewBonusPoolGroup("code-741", "label-824");
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
        MemoryBonusPoolGroupStoreFindsWhatItSaved();
    }
}
