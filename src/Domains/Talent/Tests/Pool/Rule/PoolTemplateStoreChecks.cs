namespace Acme.Workforce.Talent.Tests.Pool.Rule;

using Acme.Workforce.Talent.Pool.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PoolTemplateStoreChecks
{
    public static void MemoryPoolTemplateStoreFindsWhatItSaved()
    {
        var store = PoolTemplateStore.NewMemoryPoolTemplateStore();
        var item = PoolTemplate.NewPoolTemplate("code-260", "label-80");
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
        MemoryPoolTemplateStoreFindsWhatItSaved();
    }
}
