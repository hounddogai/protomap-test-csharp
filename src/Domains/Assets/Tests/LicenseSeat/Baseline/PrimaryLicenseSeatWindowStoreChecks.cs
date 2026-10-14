namespace Acme.Workforce.Assets.Tests.LicenseSeat.Baseline;

using Acme.Workforce.Assets.LicenseSeat.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PrimaryLicenseSeatWindowStoreChecks
{
    public static void MemoryPrimaryLicenseSeatWindowStoreFindsWhatItSaved()
    {
        var store = PrimaryLicenseSeatWindowStore.NewMemoryPrimaryLicenseSeatWindowStore();
        var item = PrimaryLicenseSeatWindow.NewPrimaryLicenseSeatWindow("code-812", "label-136");
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
        MemoryPrimaryLicenseSeatWindowStoreFindsWhatItSaved();
    }
}
