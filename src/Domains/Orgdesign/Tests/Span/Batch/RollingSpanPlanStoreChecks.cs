namespace Acme.Workforce.Orgdesign.Tests.Span.Batch;

using Acme.Workforce.Orgdesign.Span.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RollingSpanPlanStoreChecks
{
    public static void MemoryRollingSpanPlanStoreFindsWhatItSaved()
    {
        var store = RollingSpanPlanStore.NewMemoryRollingSpanPlanStore();
        var item = RollingSpanPlan.NewRollingSpanPlan("code-636", "name-94");
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
        MemoryRollingSpanPlanStoreFindsWhatItSaved();
    }
}
