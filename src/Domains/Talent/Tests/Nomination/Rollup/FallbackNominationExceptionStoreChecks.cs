namespace Acme.Workforce.Talent.Tests.Nomination.Rollup;

using Acme.Workforce.Talent.Nomination.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class FallbackNominationExceptionStoreChecks
{
    public static void MemoryFallbackNominationExceptionStoreFindsWhatItSaved()
    {
        var store = FallbackNominationExceptionStore.NewMemoryFallbackNominationExceptionStore();
        var item = FallbackNominationException.NewFallbackNominationException("code-462", "name-787");
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
        MemoryFallbackNominationExceptionStoreFindsWhatItSaved();
    }
}
