namespace Acme.Workforce.Workplace.Tests.Desk.DeskProposal;

using Acme.Workforce.Workplace.Desk.DeskProposal;

/// <summary>Checks of desk proposal code, run by the project's runner.</summary>
public static class FallbackDeskSlotStoreChecks
{
    public static void MemoryFallbackDeskSlotStoreFindsWhatItSaved()
    {
        var store = FallbackDeskSlotStore.NewMemoryFallbackDeskSlotStore();
        var item = FallbackDeskSlot.NewFallbackDeskSlot("code-171", "title-692");
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
        MemoryFallbackDeskSlotStoreFindsWhatItSaved();
    }
}
