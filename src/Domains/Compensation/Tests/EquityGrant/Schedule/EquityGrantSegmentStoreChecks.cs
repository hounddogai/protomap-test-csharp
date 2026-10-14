namespace Acme.Workforce.Compensation.Tests.EquityGrant.Schedule;

using Acme.Workforce.Compensation.EquityGrant.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class EquityGrantSegmentStoreChecks
{
    public static void MemoryEquityGrantSegmentStoreFindsWhatItSaved()
    {
        var store = EquityGrantSegmentStore.NewMemoryEquityGrantSegmentStore();
        var item = EquityGrantSegment.NewEquityGrantSegment("code-966", "name-313");
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
        MemoryEquityGrantSegmentStoreFindsWhatItSaved();
    }
}
