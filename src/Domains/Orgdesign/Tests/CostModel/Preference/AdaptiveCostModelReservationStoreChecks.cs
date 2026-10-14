namespace Acme.Workforce.Orgdesign.Tests.CostModel.Preference;

using Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class AdaptiveCostModelReservationStoreChecks
{
    public static void MemoryAdaptiveCostModelReservationStoreFindsWhatItSaved()
    {
        var store = AdaptiveCostModelReservationStore.NewMemoryAdaptiveCostModelReservationStore();
        var item = AdaptiveCostModelReservation.NewAdaptiveCostModelReservation("code-735", "name-977");
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
        MemoryAdaptiveCostModelReservationStoreFindsWhatItSaved();
    }
}
