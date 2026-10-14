namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Window;

using Acme.Workforce.Talent.SuccessionPlan.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class SuccessionPlanWindowStoreChecks
{
    public static void MemorySuccessionPlanWindowStoreFindsWhatItSaved()
    {
        var store = SuccessionPlanWindowStore.NewMemorySuccessionPlanWindowStore();
        var item = SuccessionPlanWindow.NewSuccessionPlanWindow("code-605", "label-213");
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
        MemorySuccessionPlanWindowStoreFindsWhatItSaved();
    }
}
