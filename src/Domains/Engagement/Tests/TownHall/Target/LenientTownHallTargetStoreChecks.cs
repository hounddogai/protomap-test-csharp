namespace Acme.Workforce.Engagement.Tests.TownHall.Target;

using Acme.Workforce.Engagement.TownHall.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LenientTownHallTargetStoreChecks
{
    public static void MemoryLenientTownHallTargetStoreFindsWhatItSaved()
    {
        var store = LenientTownHallTargetStore.NewMemoryLenientTownHallTargetStore();
        var item = LenientTownHallTarget.NewLenientTownHallTarget("code-533", "name-152");
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
        MemoryLenientTownHallTargetStoreFindsWhatItSaved();
    }
}
