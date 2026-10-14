namespace Acme.Workforce.Payrollops.Tests.RetroPay.Reservation;

using Acme.Workforce.Payrollops.RetroPay.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AdaptiveRetroPayCheckpointStoreChecks
{
    public static void MemoryAdaptiveRetroPayCheckpointStoreFindsWhatItSaved()
    {
        var store = AdaptiveRetroPayCheckpointStore.NewMemoryAdaptiveRetroPayCheckpointStore();
        var item = AdaptiveRetroPayCheckpoint.NewAdaptiveRetroPayCheckpoint("code-194", "name-767");
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
        MemoryAdaptiveRetroPayCheckpointStoreFindsWhatItSaved();
    }
}
