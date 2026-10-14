namespace Acme.Workforce.Workplace.Tests.Desk.Mapping;

using Acme.Workforce.Workplace.Desk.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DeskPreferenceStoreChecks
{
    public static void MemoryDeskPreferenceStoreFindsWhatItSaved()
    {
        var store = DeskPreferenceStore.NewMemoryDeskPreferenceStore();
        var item = DeskPreference.NewDeskPreference("code-865", "label-796");
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
        MemoryDeskPreferenceStoreFindsWhatItSaved();
    }
}
