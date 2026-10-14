namespace Acme.Workforce.Compensation.Tests.BonusPool.Window;

using Acme.Workforce.Compensation.BonusPool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class BonusPoolWindowStoreChecks
{
    public static void MemoryBonusPoolWindowStoreFindsWhatItSaved()
    {
        var store = BonusPoolWindowStore.NewMemoryBonusPoolWindowStore();
        var item = BonusPoolWindow.NewBonusPoolWindow("code-909", "name-112");
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
        MemoryBonusPoolWindowStoreFindsWhatItSaved();
    }
}
