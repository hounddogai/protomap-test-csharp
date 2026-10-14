namespace Acme.Workforce.Workplace.Tests.Visitor.OverrideItem;

using Acme.Workforce.Workplace.Visitor.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class VisitorOverrideStoreChecks
{
    public static void MemoryVisitorOverrideStoreFindsWhatItSaved()
    {
        var store = VisitorOverrideStore.NewMemoryVisitorOverrideStore();
        var item = VisitorOverride.NewVisitorOverride("code-24", "name-850");
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
        MemoryVisitorOverrideStoreFindsWhatItSaved();
    }
}
