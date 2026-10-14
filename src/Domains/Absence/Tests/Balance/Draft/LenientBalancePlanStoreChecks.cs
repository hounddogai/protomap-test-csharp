namespace Acme.Workforce.Absence.Tests.Balance.Draft;

using Acme.Workforce.Absence.Balance.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientBalancePlanStoreChecks
{
    public static void MemoryLenientBalancePlanStoreFindsWhatItSaved()
    {
        var store = LenientBalancePlanStore.NewMemoryLenientBalancePlanStore();
        var item = LenientBalancePlan.NewLenientBalancePlan("code-461", "name-118");
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
        MemoryLenientBalancePlanStoreFindsWhatItSaved();
    }
}
