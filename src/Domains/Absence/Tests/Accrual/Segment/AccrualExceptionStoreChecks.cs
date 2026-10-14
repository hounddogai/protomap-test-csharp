namespace Acme.Workforce.Absence.Tests.Accrual.Segment;

using Acme.Workforce.Absence.Accrual.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AccrualExceptionStoreChecks
{
    public static void MemoryAccrualExceptionStoreFindsWhatItSaved()
    {
        var store = AccrualExceptionStore.NewMemoryAccrualExceptionStore();
        var item = AccrualException.NewAccrualException("code-725", "name-291");
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
        MemoryAccrualExceptionStoreFindsWhatItSaved();
    }
}
