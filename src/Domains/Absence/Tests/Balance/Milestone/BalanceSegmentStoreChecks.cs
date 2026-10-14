namespace Acme.Workforce.Absence.Tests.Balance.Milestone;

using Acme.Workforce.Absence.Balance.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class BalanceSegmentStoreChecks
{
    public static void MemoryBalanceSegmentStoreFindsWhatItSaved()
    {
        var store = BalanceSegmentStore.NewMemoryBalanceSegmentStore();
        var item = BalanceSegment.NewBalanceSegment("code-9", "title-346");
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
        MemoryBalanceSegmentStoreFindsWhatItSaved();
    }
}
