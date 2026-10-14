namespace Acme.Workforce.Orgdesign.Tests.Headcount.Bundle;

using Acme.Workforce.Orgdesign.Headcount.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class HeadcountBundleStoreChecks
{
    public static void MemoryHeadcountBundleStoreFindsWhatItSaved()
    {
        var store = HeadcountBundleStore.NewMemoryHeadcountBundleStore();
        var item = HeadcountBundle.NewHeadcountBundle("code-460", "label-92");
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
        MemoryHeadcountBundleStoreFindsWhatItSaved();
    }
}
