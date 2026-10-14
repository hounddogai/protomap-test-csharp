namespace Acme.Workforce.Compliance.Tests.Exception.Template;

using Acme.Workforce.Compliance.Exception.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ExceptionAdjustmentStoreChecks
{
    public static void MemoryExceptionAdjustmentStoreFindsWhatItSaved()
    {
        var store = ExceptionAdjustmentStore.NewMemoryExceptionAdjustmentStore();
        var item = ExceptionAdjustment.NewExceptionAdjustment("code-459", "title-922");
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
        MemoryExceptionAdjustmentStoreFindsWhatItSaved();
    }
}
