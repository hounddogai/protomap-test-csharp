namespace Acme.Workforce.Compensation.Tests.Adjustment.Request;

using Acme.Workforce.Compensation.Adjustment.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AdjustmentExceptionStoreChecks
{
    public static void MemoryAdjustmentExceptionStoreFindsWhatItSaved()
    {
        var store = AdjustmentExceptionStore.NewMemoryAdjustmentExceptionStore();
        var item = AdjustmentException.NewAdjustmentException("code-399", "title-95");
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
        MemoryAdjustmentExceptionStoreFindsWhatItSaved();
    }
}
