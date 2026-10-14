namespace Acme.Workforce.Absence.Tests.Balance.Baseline;

using Acme.Workforce.Absence.Balance.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AdaptiveBalanceBaselineStoreChecks
{
    public static void MemoryAdaptiveBalanceBaselineStoreFindsWhatItSaved()
    {
        var store = AdaptiveBalanceBaselineStore.NewMemoryAdaptiveBalanceBaselineStore();
        var item = AdaptiveBalanceBaseline.NewAdaptiveBalanceBaseline("code-331", "title-179");
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
        MemoryAdaptiveBalanceBaselineStoreFindsWhatItSaved();
    }
}
