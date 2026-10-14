namespace Acme.Workforce.Engagement.Tests.TownHall.Exception;

using Acme.Workforce.Engagement.TownHall.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class TownHallTemplateStoreChecks
{
    public static void MemoryTownHallTemplateStoreFindsWhatItSaved()
    {
        var store = TownHallTemplateStore.NewMemoryTownHallTemplateStore();
        var item = TownHallTemplate.NewTownHallTemplate("code-796", "label-155");
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
        MemoryTownHallTemplateStoreFindsWhatItSaved();
    }
}
