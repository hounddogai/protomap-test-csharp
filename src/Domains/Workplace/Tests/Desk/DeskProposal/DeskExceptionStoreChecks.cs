namespace Acme.Workforce.Workplace.Tests.Desk.DeskProposal;

using Acme.Workforce.Workplace.Desk.DeskProposal;

/// <summary>Checks of desk proposal code, run by the project's runner.</summary>
public static class DeskExceptionStoreChecks
{
    public static void MemoryDeskExceptionStoreFindsWhatItSaved()
    {
        var store = DeskExceptionStore.NewMemoryDeskExceptionStore();
        var item = DeskException.NewDeskException("code-206", "title-474");
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
        MemoryDeskExceptionStoreFindsWhatItSaved();
    }
}
