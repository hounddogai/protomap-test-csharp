namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyReservation;

using Acme.Workforce.Assets.Warranty.WarrantyReservation;

/// <summary>Checks of warranty reservation code, run by the project's runner.</summary>
public static class WarrantyScheduleStoreChecks
{
    public static void MemoryWarrantyScheduleStoreFindsWhatItSaved()
    {
        var store = WarrantyScheduleStore.NewMemoryWarrantyScheduleStore();
        var item = WarrantySchedule.NewWarrantySchedule("code-377", "label-751");
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
        MemoryWarrantyScheduleStoreFindsWhatItSaved();
    }
}
