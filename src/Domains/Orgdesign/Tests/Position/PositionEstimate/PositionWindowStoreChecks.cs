namespace Acme.Workforce.Orgdesign.Tests.Position.PositionEstimate;

using Acme.Workforce.Orgdesign.Position.PositionEstimate;

/// <summary>Checks of position estimate code, run by the project's runner.</summary>
public static class PositionWindowStoreChecks
{
    public static void MemoryPositionWindowStoreFindsWhatItSaved()
    {
        var store = PositionWindowStore.NewMemoryPositionWindowStore();
        var item = PositionWindow.NewPositionWindow("code-836", "name-297");
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
        MemoryPositionWindowStoreFindsWhatItSaved();
    }
}
