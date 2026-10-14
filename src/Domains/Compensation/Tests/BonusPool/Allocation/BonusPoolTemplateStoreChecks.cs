namespace Acme.Workforce.Compensation.Tests.BonusPool.Allocation;

using Acme.Workforce.Compensation.BonusPool.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BonusPoolTemplateStoreChecks
{
    public static void MemoryBonusPoolTemplateStoreFindsWhatItSaved()
    {
        var store = BonusPoolTemplateStore.NewMemoryBonusPoolTemplateStore();
        var item = BonusPoolTemplate.NewBonusPoolTemplate("code-381", "title-954");
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
        MemoryBonusPoolTemplateStoreFindsWhatItSaved();
    }
}
