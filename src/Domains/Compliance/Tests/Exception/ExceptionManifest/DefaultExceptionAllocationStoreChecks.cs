namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionManifest;

using Acme.Workforce.Compliance.Exception.ExceptionManifest;

/// <summary>Checks of exception manifest code, run by the project's runner.</summary>
public static class DefaultExceptionAllocationStoreChecks
{
    public static void MemoryDefaultExceptionAllocationStoreFindsWhatItSaved()
    {
        var store = DefaultExceptionAllocationStore.NewMemoryDefaultExceptionAllocationStore();
        var item = DefaultExceptionAllocation.NewDefaultExceptionAllocation("code-341", "name-148");
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
        MemoryDefaultExceptionAllocationStoreFindsWhatItSaved();
    }
}
