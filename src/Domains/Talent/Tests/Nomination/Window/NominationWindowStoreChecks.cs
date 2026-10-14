namespace Acme.Workforce.Talent.Tests.Nomination.Window;

using Acme.Workforce.Talent.Nomination.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class NominationWindowStoreChecks
{
    public static void MemoryNominationWindowStoreFindsWhatItSaved()
    {
        var store = NominationWindowStore.NewMemoryNominationWindowStore();
        var item = NominationWindow.NewNominationWindow("code-540", "label-887");
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
        MemoryNominationWindowStoreFindsWhatItSaved();
    }
}
