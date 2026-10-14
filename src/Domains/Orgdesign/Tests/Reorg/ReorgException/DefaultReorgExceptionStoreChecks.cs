namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgException;

using Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>Checks of reorg exception code, run by the project's runner.</summary>
public static class DefaultReorgExceptionStoreChecks
{
    public static void MemoryDefaultReorgExceptionStoreFindsWhatItSaved()
    {
        var store = DefaultReorgExceptionStore.NewMemoryDefaultReorgExceptionStore();
        var item = DefaultReorgException.NewDefaultReorgException("code-151", "title-650");
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
        MemoryDefaultReorgExceptionStoreFindsWhatItSaved();
    }
}
