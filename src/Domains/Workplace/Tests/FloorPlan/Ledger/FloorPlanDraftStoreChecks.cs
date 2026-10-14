namespace Acme.Workforce.Workplace.Tests.FloorPlan.Ledger;

using Acme.Workforce.Workplace.FloorPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class FloorPlanDraftStoreChecks
{
    public static void MemoryFloorPlanDraftStoreFindsWhatItSaved()
    {
        var store = FloorPlanDraftStore.NewMemoryFloorPlanDraftStore();
        var item = FloorPlanDraft.NewFloorPlanDraft("code-862", "name-343");
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
        MemoryFloorPlanDraftStoreFindsWhatItSaved();
    }
}
