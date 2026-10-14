namespace Acme.Workforce.Assets.Tests.LicenseSeat.Summary;

using Acme.Workforce.Assets.LicenseSeat.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class LicenseSeatBudgetStoreChecks
{
    public static void MemoryLicenseSeatBudgetStoreFindsWhatItSaved()
    {
        var store = LicenseSeatBudgetStore.NewMemoryLicenseSeatBudgetStore();
        var item = LicenseSeatBudget.NewLicenseSeatBudget("code-441", "label-578");
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
        MemoryLicenseSeatBudgetStoreFindsWhatItSaved();
    }
}
