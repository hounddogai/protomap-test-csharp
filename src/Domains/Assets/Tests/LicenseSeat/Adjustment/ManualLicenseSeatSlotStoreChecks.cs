namespace Acme.Workforce.Assets.Tests.LicenseSeat.Adjustment;

using Acme.Workforce.Assets.LicenseSeat.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class ManualLicenseSeatSlotStoreChecks
{
    public static void MemoryManualLicenseSeatSlotStoreFindsWhatItSaved()
    {
        var store = ManualLicenseSeatSlotStore.NewMemoryManualLicenseSeatSlotStore();
        var item = ManualLicenseSeatSlot.NewManualLicenseSeatSlot("code-291", "name-887");
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
        MemoryManualLicenseSeatSlotStoreFindsWhatItSaved();
    }
}
