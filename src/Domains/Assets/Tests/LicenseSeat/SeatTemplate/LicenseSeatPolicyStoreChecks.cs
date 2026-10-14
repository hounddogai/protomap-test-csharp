namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatTemplate;

using Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

/// <summary>Checks of seat template code, run by the project's runner.</summary>
public static class LicenseSeatPolicyStoreChecks
{
    public static void MemoryLicenseSeatPolicyStoreFindsWhatItSaved()
    {
        var store = LicenseSeatPolicyStore.NewMemoryLicenseSeatPolicyStore();
        var item = LicenseSeatPolicy.NewLicenseSeatPolicy("code-492", "title-208");
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
        MemoryLicenseSeatPolicyStoreFindsWhatItSaved();
    }
}
