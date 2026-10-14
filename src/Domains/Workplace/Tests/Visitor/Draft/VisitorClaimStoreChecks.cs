namespace Acme.Workforce.Workplace.Tests.Visitor.Draft;

using Acme.Workforce.Workplace.Visitor.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class VisitorClaimStoreChecks
{
    public static void MemoryVisitorClaimStoreFindsWhatItSaved()
    {
        var store = VisitorClaimStore.NewMemoryVisitorClaimStore();
        var item = VisitorClaim.NewVisitorClaim("code-857", "label-600");
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
        MemoryVisitorClaimStoreFindsWhatItSaved();
    }
}
