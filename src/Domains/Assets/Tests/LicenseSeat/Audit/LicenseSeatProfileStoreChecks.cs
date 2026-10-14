namespace Acme.Workforce.Assets.Tests.LicenseSeat.Audit;

using Acme.Workforce.Assets.LicenseSeat.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LicenseSeatProfileStoreChecks
{
    public static void MemoryLicenseSeatProfileStoreFindsWhatItSaved()
    {
        var store = LicenseSeatProfileStore.NewMemoryLicenseSeatProfileStore();
        var item = LicenseSeatProfile.NewLicenseSeatProfile("code-979", "title-747");
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
        MemoryLicenseSeatProfileStoreFindsWhatItSaved();
    }
}
