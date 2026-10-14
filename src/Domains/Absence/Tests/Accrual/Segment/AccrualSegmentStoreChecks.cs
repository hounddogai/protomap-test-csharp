namespace Acme.Workforce.Absence.Tests.Accrual.Segment;

using Acme.Workforce.Absence.Accrual.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AccrualSegmentStoreChecks
{
    public static void MemoryAccrualSegmentStoreFindsWhatItSaved()
    {
        var store = AccrualSegmentStore.NewMemoryAccrualSegmentStore();
        var item = AccrualSegment.NewAccrualSegment("code-884", "name-632");
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
        MemoryAccrualSegmentStoreFindsWhatItSaved();
    }
}
