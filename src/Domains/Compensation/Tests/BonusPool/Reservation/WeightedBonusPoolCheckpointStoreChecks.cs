namespace Acme.Workforce.Compensation.Tests.BonusPool.Reservation;

using Acme.Workforce.Compensation.BonusPool.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WeightedBonusPoolCheckpointStoreChecks
{
    public static void MemoryWeightedBonusPoolCheckpointStoreFindsWhatItSaved()
    {
        var store = WeightedBonusPoolCheckpointStore.NewMemoryWeightedBonusPoolCheckpointStore();
        var item = WeightedBonusPoolCheckpoint.NewWeightedBonusPoolCheckpoint("code-238", "label-674");
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
        MemoryWeightedBonusPoolCheckpointStoreFindsWhatItSaved();
    }
}
