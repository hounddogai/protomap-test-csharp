namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorOverride;

using Acme.Workforce.Workplace.Visitor.VisitorOverride;

/// <summary>Checks of visitor override code, run by the project's runner.</summary>
public static class PrimaryVisitorOverrideStoreChecks
{
    public static void MemoryPrimaryVisitorOverrideStoreFindsWhatItSaved()
    {
        var store = PrimaryVisitorOverrideStore.NewMemoryPrimaryVisitorOverrideStore();
        var item = PrimaryVisitorOverride.NewPrimaryVisitorOverride("code-353", "name-807");
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
        MemoryPrimaryVisitorOverrideStoreFindsWhatItSaved();
    }
}
