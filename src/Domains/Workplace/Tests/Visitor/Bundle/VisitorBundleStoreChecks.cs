namespace Acme.Workforce.Workplace.Tests.Visitor.Bundle;

using Acme.Workforce.Workplace.Visitor.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class VisitorBundleStoreChecks
{
    public static void MemoryVisitorBundleStoreFindsWhatItSaved()
    {
        var store = VisitorBundleStore.NewMemoryVisitorBundleStore();
        var item = VisitorBundle.NewVisitorBundle("code-671", "name-590");
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
        MemoryVisitorBundleStoreFindsWhatItSaved();
    }
}
