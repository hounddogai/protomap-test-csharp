namespace Acme.Workforce.Orgdesign.Tests.Position.PositionThreshold;

using Acme.Workforce.Orgdesign.Position.PositionThreshold;

/// <summary>Checks of position threshold code, run by the project's runner.</summary>
public static class PositionDraftStoreChecks
{
    public static void MemoryPositionDraftStoreFindsWhatItSaved()
    {
        var store = PositionDraftStore.NewMemoryPositionDraftStore();
        var item = PositionDraft.NewPositionDraft("code-864", "name-359");
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
        MemoryPositionDraftStoreFindsWhatItSaved();
    }
}
