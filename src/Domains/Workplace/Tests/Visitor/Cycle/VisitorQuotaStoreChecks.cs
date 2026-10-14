namespace Acme.Workforce.Workplace.Tests.Visitor.Cycle;

using Acme.Workforce.Workplace.Visitor.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class VisitorQuotaStoreChecks
{
    public static void MemoryVisitorQuotaStoreFindsWhatItSaved()
    {
        var store = VisitorQuotaStore.NewMemoryVisitorQuotaStore();
        var item = VisitorQuota.NewVisitorQuota("code-494", "label-594");
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
        MemoryVisitorQuotaStoreFindsWhatItSaved();
    }
}
