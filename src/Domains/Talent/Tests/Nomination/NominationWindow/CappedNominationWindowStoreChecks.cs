namespace Acme.Workforce.Talent.Tests.Nomination.NominationWindow;

using Acme.Workforce.Talent.Nomination.NominationWindow;

/// <summary>Checks of nomination window code, run by the project's runner.</summary>
public static class CappedNominationWindowStoreChecks
{
    public static void MemoryCappedNominationWindowStoreFindsWhatItSaved()
    {
        var store = CappedNominationWindowStore.NewMemoryCappedNominationWindowStore();
        var item = CappedNominationWindow.NewCappedNominationWindow("code-65", "label-799");
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
        MemoryCappedNominationWindowStoreFindsWhatItSaved();
    }
}
