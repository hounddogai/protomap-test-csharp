namespace Acme.Workforce.Absence.Tests.Accrual.Checkpoint;

using Acme.Workforce.Absence.Accrual.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class ManualAccrualCheckpointStoreChecks
{
    public static void MemoryManualAccrualCheckpointStoreFindsWhatItSaved()
    {
        var store = ManualAccrualCheckpointStore.NewMemoryManualAccrualCheckpointStore();
        var item = ManualAccrualCheckpoint.NewManualAccrualCheckpoint("code-11", "title-960");
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
        MemoryManualAccrualCheckpointStoreFindsWhatItSaved();
    }
}
