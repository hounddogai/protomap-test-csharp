namespace Acme.Workforce.Talent.Tests.Pool.Adjustment;

using Acme.Workforce.Talent.Pool.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class GlobalPoolBaselineStoreChecks
{
    public static void MemoryGlobalPoolBaselineStoreFindsWhatItSaved()
    {
        var store = GlobalPoolBaselineStore.NewMemoryGlobalPoolBaselineStore();
        var item = GlobalPoolBaseline.NewGlobalPoolBaseline("code-309", "name-983");
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
        MemoryGlobalPoolBaselineStoreFindsWhatItSaved();
    }
}
