namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Ledger;

using Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SuccessionPlanRecordStoreChecks
{
    public static void MemorySuccessionPlanRecordStoreFindsWhatItSaved()
    {
        var store = SuccessionPlanRecordStore.NewMemorySuccessionPlanRecordStore();
        var item = SuccessionPlanRecord.NewSuccessionPlanRecord("code-664", "name-570");
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
        MemorySuccessionPlanRecordStoreFindsWhatItSaved();
    }
}
