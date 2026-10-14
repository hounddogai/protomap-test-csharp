namespace Acme.Workforce.Absence.Tests.ReturnPlan.Window;

using Acme.Workforce.Absence.ReturnPlan.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TieredReturnPlanItemStoreChecks
{
    public static void MemoryTieredReturnPlanItemStoreFindsWhatItSaved()
    {
        var store = TieredReturnPlanItemStore.NewMemoryTieredReturnPlanItemStore();
        var item = TieredReturnPlanItem.NewTieredReturnPlanItem("code-76", "title-175");
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
        MemoryTieredReturnPlanItemStoreFindsWhatItSaved();
    }
}
