namespace Acme.Workforce.Assets.Tests.LicenseSeat.Adjustment;

using Acme.Workforce.Assets.LicenseSeat.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LicenseSeatAdjustmentStoreChecks
{
    public static void MemoryLicenseSeatAdjustmentStoreFindsWhatItSaved()
    {
        var store = LicenseSeatAdjustmentStore.NewMemoryLicenseSeatAdjustmentStore();
        var item = LicenseSeatAdjustment.NewLicenseSeatAdjustment("code-640", "name-407");
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
        MemoryLicenseSeatAdjustmentStoreFindsWhatItSaved();
    }
}
