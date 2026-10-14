namespace Acme.Workforce.Orgdesign.Tests.Position.Mapping;

using Acme.Workforce.Orgdesign.Position.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class PositionTemplateStoreChecks
{
    public static void MemoryPositionTemplateStoreFindsWhatItSaved()
    {
        var store = PositionTemplateStore.NewMemoryPositionTemplateStore();
        var item = PositionTemplate.NewPositionTemplate("code-474", "name-92");
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
        MemoryPositionTemplateStoreFindsWhatItSaved();
    }
}
