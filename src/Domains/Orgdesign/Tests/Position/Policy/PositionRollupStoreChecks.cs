namespace Acme.Workforce.Orgdesign.Tests.Position.Policy;

using Acme.Workforce.Orgdesign.Position.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class PositionRollupStoreChecks
{
    public static void MemoryPositionRollupStoreFindsWhatItSaved()
    {
        var store = PositionRollupStore.NewMemoryPositionRollupStore();
        var item = PositionRollup.NewPositionRollup("code-885", "title-89");
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
        MemoryPositionRollupStoreFindsWhatItSaved();
    }
}
