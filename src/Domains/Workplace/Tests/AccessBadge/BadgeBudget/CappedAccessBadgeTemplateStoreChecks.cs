namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeBudget;

using Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

/// <summary>Checks of badge budget code, run by the project's runner.</summary>
public static class CappedAccessBadgeTemplateStoreChecks
{
    public static void MemoryCappedAccessBadgeTemplateStoreFindsWhatItSaved()
    {
        var store = CappedAccessBadgeTemplateStore.NewMemoryCappedAccessBadgeTemplateStore();
        var item = CappedAccessBadgeTemplate.NewCappedAccessBadgeTemplate("code-50", "title-626");
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
        MemoryCappedAccessBadgeTemplateStoreFindsWhatItSaved();
    }
}
