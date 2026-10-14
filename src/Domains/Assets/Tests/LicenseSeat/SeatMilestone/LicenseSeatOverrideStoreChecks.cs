namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatMilestone;

using Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

/// <summary>Checks of seat milestone code, run by the project's runner.</summary>
public static class LicenseSeatOverrideStoreChecks
{
    public static void MemoryLicenseSeatOverrideStoreFindsWhatItSaved()
    {
        var store = LicenseSeatOverrideStore.NewMemoryLicenseSeatOverrideStore();
        var item = LicenseSeatOverride.NewLicenseSeatOverride("code-417", "label-382");
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
        MemoryLicenseSeatOverrideStoreFindsWhatItSaved();
    }
}
