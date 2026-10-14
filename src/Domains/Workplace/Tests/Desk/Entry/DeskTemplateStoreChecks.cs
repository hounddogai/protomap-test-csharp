namespace Acme.Workforce.Workplace.Tests.Desk.Entry;

using Acme.Workforce.Workplace.Desk.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class DeskTemplateStoreChecks
{
    public static void MemoryDeskTemplateStoreFindsWhatItSaved()
    {
        var store = DeskTemplateStore.NewMemoryDeskTemplateStore();
        var item = DeskTemplate.NewDeskTemplate("code-183", "label-155");
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
        MemoryDeskTemplateStoreFindsWhatItSaved();
    }
}
