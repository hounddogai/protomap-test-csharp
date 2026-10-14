namespace Acme.Workforce.Compensation.Tests.EquityGrant.Ledger;

using Acme.Workforce.Compensation.EquityGrant.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BatchedEquityGrantClaimStoreChecks
{
    public static void MemoryBatchedEquityGrantClaimStoreFindsWhatItSaved()
    {
        var store = BatchedEquityGrantClaimStore.NewMemoryBatchedEquityGrantClaimStore();
        var item = BatchedEquityGrantClaim.NewBatchedEquityGrantClaim("code-160", "title-31");
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
        MemoryBatchedEquityGrantClaimStoreFindsWhatItSaved();
    }
}
