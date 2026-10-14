namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatMilestone;

using Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

/// <summary>Checks of seat milestone code, run by the project's runner.</summary>
public static class LicenseSeatAssignmentStoreChecks
{
    public static void MemoryLicenseSeatAssignmentStoreFindsWhatItSaved()
    {
        var store = LicenseSeatAssignmentStore.NewMemoryLicenseSeatAssignmentStore();
        var item = LicenseSeatAssignment.NewLicenseSeatAssignment("code-889", "label-143");
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
        MemoryLicenseSeatAssignmentStoreFindsWhatItSaved();
    }
}
