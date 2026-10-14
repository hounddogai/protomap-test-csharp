namespace Acme.Workforce.Engagement.Tests.ActionPlan.Slot;

using Acme.Workforce.Engagement.ActionPlan.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ActionPlanSlotStoreChecks
{
    public static void MemoryActionPlanSlotStoreFindsWhatItSaved()
    {
        var store = ActionPlanSlotStore.NewMemoryActionPlanSlotStore();
        var item = ActionPlanSlot.NewActionPlanSlot("code-833", "name-304");
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
        MemoryActionPlanSlotStoreFindsWhatItSaved();
    }
}
