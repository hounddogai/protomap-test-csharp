namespace Acme.Workforce.Talent.Tests.Mobility.Window;

using Acme.Workforce.Talent.Mobility.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class MobilityWindowStoreChecks
{
    public static void MemoryMobilityWindowStoreFindsWhatItSaved()
    {
        var store = MobilityWindowStore.NewMemoryMobilityWindowStore();
        var item = MobilityWindow.NewMobilityWindow("code-781", "label-141");
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
        MemoryMobilityWindowStoreFindsWhatItSaved();
    }
}
