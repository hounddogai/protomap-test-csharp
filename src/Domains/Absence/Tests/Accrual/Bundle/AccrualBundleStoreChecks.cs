namespace Acme.Workforce.Absence.Tests.Accrual.Bundle;

using Acme.Workforce.Absence.Accrual.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class AccrualBundleStoreChecks
{
    public static void MemoryAccrualBundleStoreFindsWhatItSaved()
    {
        var store = AccrualBundleStore.NewMemoryAccrualBundleStore();
        var item = AccrualBundle.NewAccrualBundle("code-858", "name-894");
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
        MemoryAccrualBundleStoreFindsWhatItSaved();
    }
}
