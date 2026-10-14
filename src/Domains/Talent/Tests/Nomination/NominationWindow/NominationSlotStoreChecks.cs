namespace Acme.Workforce.Talent.Tests.Nomination.NominationWindow;

using Acme.Workforce.Talent.Nomination.NominationWindow;

/// <summary>Checks of nomination window code, run by the project's runner.</summary>
public static class NominationSlotStoreChecks
{
    public static void MemoryNominationSlotStoreFindsWhatItSaved()
    {
        var store = NominationSlotStore.NewMemoryNominationSlotStore();
        var item = NominationSlot.NewNominationSlot("code-4", "title-697");
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
        MemoryNominationSlotStoreFindsWhatItSaved();
    }
}
