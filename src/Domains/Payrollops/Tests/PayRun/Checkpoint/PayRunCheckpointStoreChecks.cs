namespace Acme.Workforce.Payrollops.Tests.PayRun.Checkpoint;

using Acme.Workforce.Payrollops.PayRun.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class PayRunCheckpointStoreChecks
{
    public static void MemoryPayRunCheckpointStoreFindsWhatItSaved()
    {
        var store = PayRunCheckpointStore.NewMemoryPayRunCheckpointStore();
        var item = PayRunCheckpoint.NewPayRunCheckpoint("code-528", "label-87");
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
        MemoryPayRunCheckpointStoreFindsWhatItSaved();
    }
}
