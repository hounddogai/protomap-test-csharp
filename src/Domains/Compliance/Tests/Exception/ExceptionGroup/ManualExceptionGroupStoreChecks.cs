namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionGroup;

using Acme.Workforce.Compliance.Exception.ExceptionGroup;

/// <summary>Checks of exception group code, run by the project's runner.</summary>
public static class ManualExceptionGroupStoreChecks
{
    public static void MemoryManualExceptionGroupStoreFindsWhatItSaved()
    {
        var store = ManualExceptionGroupStore.NewMemoryManualExceptionGroupStore();
        var item = ManualExceptionGroup.NewManualExceptionGroup("code-662", "name-338");
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
        MemoryManualExceptionGroupStoreFindsWhatItSaved();
    }
}
