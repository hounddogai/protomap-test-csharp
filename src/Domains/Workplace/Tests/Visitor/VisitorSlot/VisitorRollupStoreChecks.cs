namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorSlot;

using Acme.Workforce.Workplace.Visitor.VisitorSlot;

/// <summary>Checks of visitor slot code, run by the project's runner.</summary>
public static class VisitorRollupStoreChecks
{
    public static void MemoryVisitorRollupStoreFindsWhatItSaved()
    {
        var store = VisitorRollupStore.NewMemoryVisitorRollupStore();
        var item = VisitorRollup.NewVisitorRollup("code-501", "name-739");
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
        MemoryVisitorRollupStoreFindsWhatItSaved();
    }
}
