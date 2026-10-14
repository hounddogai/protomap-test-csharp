namespace Acme.Workforce.Compensation.Tests.PayBand.Milestone;

using Acme.Workforce.Compensation.PayBand.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class PrimaryPayBandCheckpointStoreChecks
{
    public static void MemoryPrimaryPayBandCheckpointStoreFindsWhatItSaved()
    {
        var store = PrimaryPayBandCheckpointStore.NewMemoryPrimaryPayBandCheckpointStore();
        var item = PrimaryPayBandCheckpoint.NewPrimaryPayBandCheckpoint("code-226", "label-380");
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
        MemoryPrimaryPayBandCheckpointStoreFindsWhatItSaved();
    }
}
