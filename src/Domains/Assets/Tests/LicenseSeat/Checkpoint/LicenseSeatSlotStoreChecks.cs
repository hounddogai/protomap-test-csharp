namespace Acme.Workforce.Assets.Tests.LicenseSeat.Checkpoint;

using Acme.Workforce.Assets.LicenseSeat.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LicenseSeatSlotStoreChecks
{
    public static void MemoryLicenseSeatSlotStoreFindsWhatItSaved()
    {
        var store = LicenseSeatSlotStore.NewMemoryLicenseSeatSlotStore();
        var item = LicenseSeatSlot.NewLicenseSeatSlot("code-237", "name-803");
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
        MemoryLicenseSeatSlotStoreFindsWhatItSaved();
    }
}
