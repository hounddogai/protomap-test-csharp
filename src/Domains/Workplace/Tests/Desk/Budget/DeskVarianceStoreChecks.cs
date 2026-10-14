namespace Acme.Workforce.Workplace.Tests.Desk.Budget;

using Acme.Workforce.Workplace.Desk.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class DeskVarianceStoreChecks
{
    public static void MemoryDeskVarianceStoreFindsWhatItSaved()
    {
        var store = DeskVarianceStore.NewMemoryDeskVarianceStore();
        var item = DeskVariance.NewDeskVariance("code-207", "name-369");
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
        MemoryDeskVarianceStoreFindsWhatItSaved();
    }
}
