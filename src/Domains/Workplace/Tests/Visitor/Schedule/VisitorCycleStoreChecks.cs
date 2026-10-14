namespace Acme.Workforce.Workplace.Tests.Visitor.Schedule;

using Acme.Workforce.Workplace.Visitor.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class VisitorCycleStoreChecks
{
    public static void MemoryVisitorCycleStoreFindsWhatItSaved()
    {
        var store = VisitorCycleStore.NewMemoryVisitorCycleStore();
        var item = VisitorCycle.NewVisitorCycle("code-321", "name-272");
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
        MemoryVisitorCycleStoreFindsWhatItSaved();
    }
}
