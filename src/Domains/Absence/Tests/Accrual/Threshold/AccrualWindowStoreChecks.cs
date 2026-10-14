namespace Acme.Workforce.Absence.Tests.Accrual.Threshold;

using Acme.Workforce.Absence.Accrual.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AccrualWindowStoreChecks
{
    public static void MemoryAccrualWindowStoreFindsWhatItSaved()
    {
        var store = AccrualWindowStore.NewMemoryAccrualWindowStore();
        var item = AccrualWindow.NewAccrualWindow("code-890", "title-660");
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
        MemoryAccrualWindowStoreFindsWhatItSaved();
    }
}
