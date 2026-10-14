namespace Acme.Workforce.Absence.Tests.Accrual.Baseline;

using Acme.Workforce.Absence.Accrual.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AccrualBaselineStoreChecks
{
    public static void MemoryAccrualBaselineStoreFindsWhatItSaved()
    {
        var store = AccrualBaselineStore.NewMemoryAccrualBaselineStore();
        var item = AccrualBaseline.NewAccrualBaseline("code-100", "title-189");
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
        MemoryAccrualBaselineStoreFindsWhatItSaved();
    }
}
