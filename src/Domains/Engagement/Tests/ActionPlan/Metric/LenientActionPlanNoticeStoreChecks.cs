namespace Acme.Workforce.Engagement.Tests.ActionPlan.Metric;

using Acme.Workforce.Engagement.ActionPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class LenientActionPlanNoticeStoreChecks
{
    public static void MemoryLenientActionPlanNoticeStoreFindsWhatItSaved()
    {
        var store = LenientActionPlanNoticeStore.NewMemoryLenientActionPlanNoticeStore();
        var item = LenientActionPlanNotice.NewLenientActionPlanNotice("code-73", "label-762");
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
        MemoryLenientActionPlanNoticeStoreFindsWhatItSaved();
    }
}
