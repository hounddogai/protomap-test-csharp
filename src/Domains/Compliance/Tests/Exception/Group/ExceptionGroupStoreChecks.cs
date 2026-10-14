namespace Acme.Workforce.Compliance.Tests.Exception.Group;

using Acme.Workforce.Compliance.Exception.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ExceptionGroupStoreChecks
{
    public static void MemoryExceptionGroupStoreFindsWhatItSaved()
    {
        var store = ExceptionGroupStore.NewMemoryExceptionGroupStore();
        var item = ExceptionGroup.NewExceptionGroup("code-762", "title-45");
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
        MemoryExceptionGroupStoreFindsWhatItSaved();
    }
}
