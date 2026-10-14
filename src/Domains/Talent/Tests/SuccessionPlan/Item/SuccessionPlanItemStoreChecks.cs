namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Item;

using Acme.Workforce.Talent.SuccessionPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class SuccessionPlanItemStoreChecks
{
    public static void MemorySuccessionPlanItemStoreFindsWhatItSaved()
    {
        var store = SuccessionPlanItemStore.NewMemorySuccessionPlanItemStore();
        var item = SuccessionPlanItem.NewSuccessionPlanItem("code-790", "title-634");
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
        MemorySuccessionPlanItemStoreFindsWhatItSaved();
    }
}
