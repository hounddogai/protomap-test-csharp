namespace Acme.Workforce.Workplace.Tests.Visitor.Group;

using Acme.Workforce.Workplace.Visitor.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class VisitorProfileStoreChecks
{
    public static void MemoryVisitorProfileStoreFindsWhatItSaved()
    {
        var store = VisitorProfileStore.NewMemoryVisitorProfileStore();
        var item = VisitorProfile.NewVisitorProfile("code-942", "label-876");
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
        MemoryVisitorProfileStoreFindsWhatItSaved();
    }
}
