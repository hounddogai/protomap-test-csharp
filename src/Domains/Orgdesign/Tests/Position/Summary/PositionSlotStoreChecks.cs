namespace Acme.Workforce.Orgdesign.Tests.Position.Summary;

using Acme.Workforce.Orgdesign.Position.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PositionSlotStoreChecks
{
    public static void MemoryPositionSlotStoreFindsWhatItSaved()
    {
        var store = PositionSlotStore.NewMemoryPositionSlotStore();
        var item = PositionSlot.NewPositionSlot("code-319", "name-598");
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
        MemoryPositionSlotStoreFindsWhatItSaved();
    }
}
