namespace Acme.Workforce.Absence.Tests.Accrual.Bundle;

using Acme.Workforce.Absence.Accrual.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class AdaptiveAccrualBundleStoreChecks
{
    public static void MemoryAdaptiveAccrualBundleStoreFindsWhatItSaved()
    {
        var store = AdaptiveAccrualBundleStore.NewMemoryAdaptiveAccrualBundleStore();
        var item = AdaptiveAccrualBundle.NewAdaptiveAccrualBundle("code-81", "label-755");
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
        MemoryAdaptiveAccrualBundleStoreFindsWhatItSaved();
    }
}
