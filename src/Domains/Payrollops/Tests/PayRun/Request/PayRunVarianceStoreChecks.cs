namespace Acme.Workforce.Payrollops.Tests.PayRun.Request;

using Acme.Workforce.Payrollops.PayRun.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class PayRunVarianceStoreChecks
{
    public static void MemoryPayRunVarianceStoreFindsWhatItSaved()
    {
        var store = PayRunVarianceStore.NewMemoryPayRunVarianceStore();
        var item = PayRunVariance.NewPayRunVariance("code-849", "title-958");
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
        MemoryPayRunVarianceStoreFindsWhatItSaved();
    }
}
