namespace Acme.Workforce.Workplace.Tests.Visitor.Checkpoint;

using Acme.Workforce.Workplace.Visitor.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LenientVisitorPreferenceStoreChecks
{
    public static void MemoryLenientVisitorPreferenceStoreFindsWhatItSaved()
    {
        var store = LenientVisitorPreferenceStore.NewMemoryLenientVisitorPreferenceStore();
        var item = LenientVisitorPreference.NewLenientVisitorPreference("code-739", "label-558");
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
        MemoryLenientVisitorPreferenceStoreFindsWhatItSaved();
    }
}
