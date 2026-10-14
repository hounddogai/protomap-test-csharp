namespace Acme.Workforce.Orgdesign.Tests.Reorg.Bundle;

using Acme.Workforce.Orgdesign.Reorg.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ReorgPreferenceStoreChecks
{
    public static void MemoryReorgPreferenceStoreFindsWhatItSaved()
    {
        var store = ReorgPreferenceStore.NewMemoryReorgPreferenceStore();
        var item = ReorgPreference.NewReorgPreference("code-783", "title-96");
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
        MemoryReorgPreferenceStoreFindsWhatItSaved();
    }
}
