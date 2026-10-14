namespace Acme.Workforce.Payrollops.Tests.PayRun.Bundle;

using Acme.Workforce.Payrollops.PayRun.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PayRunExceptionStoreChecks
{
    public static void MemoryPayRunExceptionStoreFindsWhatItSaved()
    {
        var store = PayRunExceptionStore.NewMemoryPayRunExceptionStore();
        var item = PayRunException.NewPayRunException("code-314", "label-372");
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
        MemoryPayRunExceptionStoreFindsWhatItSaved();
    }
}
