namespace Acme.Workforce.Compensation.Tests.BonusPool.Exception;

using Acme.Workforce.Compensation.BonusPool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class AdaptiveBonusPoolReservationStoreChecks
{
    public static void MemoryAdaptiveBonusPoolReservationStoreFindsWhatItSaved()
    {
        var store = AdaptiveBonusPoolReservationStore.NewMemoryAdaptiveBonusPoolReservationStore();
        var item = AdaptiveBonusPoolReservation.NewAdaptiveBonusPoolReservation("code-22", "label-944");
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
        MemoryAdaptiveBonusPoolReservationStoreFindsWhatItSaved();
    }
}
