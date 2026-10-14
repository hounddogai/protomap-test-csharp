namespace Acme.Workforce.Workplace.Tests.Desk.Mapping;

using Acme.Workforce.Workplace.Desk.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DeskMappingStoreChecks
{
    public static void MemoryDeskMappingStoreFindsWhatItSaved()
    {
        var store = DeskMappingStore.NewMemoryDeskMappingStore();
        var item = DeskMapping.NewDeskMapping("code-662", "title-988");
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
        MemoryDeskMappingStoreFindsWhatItSaved();
    }
}
