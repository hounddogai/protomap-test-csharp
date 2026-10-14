namespace Acme.Workforce.Payrollops.Tests.RetroPay.Assignment;

using Acme.Workforce.Payrollops.RetroPay.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class LenientRetroPayExceptionStoreChecks
{
    public static void MemoryLenientRetroPayExceptionStoreFindsWhatItSaved()
    {
        var store = LenientRetroPayExceptionStore.NewMemoryLenientRetroPayExceptionStore();
        var item = LenientRetroPayException.NewLenientRetroPayException("code-997", "label-54");
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
        MemoryLenientRetroPayExceptionStoreFindsWhatItSaved();
    }
}
