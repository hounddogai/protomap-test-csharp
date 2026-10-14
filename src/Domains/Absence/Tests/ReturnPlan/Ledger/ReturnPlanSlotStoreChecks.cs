namespace Acme.Workforce.Absence.Tests.ReturnPlan.Ledger;

using Acme.Workforce.Absence.ReturnPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ReturnPlanSlotStoreChecks
{
    public static void MemoryReturnPlanSlotStoreFindsWhatItSaved()
    {
        var store = ReturnPlanSlotStore.NewMemoryReturnPlanSlotStore();
        var item = ReturnPlanSlot.NewReturnPlanSlot("code-646", "label-425");
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
        MemoryReturnPlanSlotStoreFindsWhatItSaved();
    }
}
