namespace Acme.Workforce.Orgdesign.Tests.Headcount.Budget;

using Acme.Workforce.Orgdesign.Headcount.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class BatchedHeadcountTemplateStoreChecks
{
    public static void MemoryBatchedHeadcountTemplateStoreFindsWhatItSaved()
    {
        var store = BatchedHeadcountTemplateStore.NewMemoryBatchedHeadcountTemplateStore();
        var item = BatchedHeadcountTemplate.NewBatchedHeadcountTemplate("code-9", "name-529");
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
        MemoryBatchedHeadcountTemplateStoreFindsWhatItSaved();
    }
}
