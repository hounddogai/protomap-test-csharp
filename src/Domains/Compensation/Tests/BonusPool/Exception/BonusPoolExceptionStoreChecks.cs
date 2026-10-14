namespace Acme.Workforce.Compensation.Tests.BonusPool.Exception;

using Acme.Workforce.Compensation.BonusPool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class BonusPoolExceptionStoreChecks
{
    public static void MemoryBonusPoolExceptionStoreFindsWhatItSaved()
    {
        var store = BonusPoolExceptionStore.NewMemoryBonusPoolExceptionStore();
        var item = BonusPoolException.NewBonusPoolException("code-71", "label-635");
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
        MemoryBonusPoolExceptionStoreFindsWhatItSaved();
    }
}
