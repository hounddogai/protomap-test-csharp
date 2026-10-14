namespace Acme.Workforce.Assets.Tests.LicenseSeat.Assignment;

using Acme.Workforce.Assets.LicenseSeat.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class GlobalLicenseSeatAssignmentStoreChecks
{
    public static void MemoryGlobalLicenseSeatAssignmentStoreFindsWhatItSaved()
    {
        var store = GlobalLicenseSeatAssignmentStore.NewMemoryGlobalLicenseSeatAssignmentStore();
        var item = GlobalLicenseSeatAssignment.NewGlobalLicenseSeatAssignment("code-743", "name-27");
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
        MemoryGlobalLicenseSeatAssignmentStoreFindsWhatItSaved();
    }
}
