namespace Acme.Workforce.Assets.Tests.LicenseSeat.Mapping;

using Acme.Workforce.Assets.LicenseSeat.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class FallbackLicenseSeatBudgetStoreChecks
{
    public static void MemoryFallbackLicenseSeatBudgetStoreFindsWhatItSaved()
    {
        var store = FallbackLicenseSeatBudgetStore.NewMemoryFallbackLicenseSeatBudgetStore();
        var item = FallbackLicenseSeatBudget.NewFallbackLicenseSeatBudget("code-877", "title-400");
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
        MemoryFallbackLicenseSeatBudgetStoreFindsWhatItSaved();
    }
}
