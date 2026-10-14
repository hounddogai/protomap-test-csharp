namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantLedger;

using Acme.Workforce.Compensation.EquityGrant.GrantLedger;

/// <summary>Checks of grant ledger code, run by the project's runner.</summary>
public static class StrictEquityGrantMilestoneStoreChecks
{
    public static void MemoryStrictEquityGrantMilestoneStoreFindsWhatItSaved()
    {
        var store = StrictEquityGrantMilestoneStore.NewMemoryStrictEquityGrantMilestoneStore();
        var item = StrictEquityGrantMilestone.NewStrictEquityGrantMilestone("code-211", "title-786");
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
        MemoryStrictEquityGrantMilestoneStoreFindsWhatItSaved();
    }
}
