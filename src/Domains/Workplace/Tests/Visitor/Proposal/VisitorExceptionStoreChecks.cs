namespace Acme.Workforce.Workplace.Tests.Visitor.Proposal;

using Acme.Workforce.Workplace.Visitor.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class VisitorExceptionStoreChecks
{
    public static void MemoryVisitorExceptionStoreFindsWhatItSaved()
    {
        var store = VisitorExceptionStore.NewMemoryVisitorExceptionStore();
        var item = VisitorException.NewVisitorException("code-826", "title-297");
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
        MemoryVisitorExceptionStoreFindsWhatItSaved();
    }
}
