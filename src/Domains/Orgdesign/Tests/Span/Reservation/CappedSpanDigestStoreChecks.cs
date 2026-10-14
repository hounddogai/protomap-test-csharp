namespace Acme.Workforce.Orgdesign.Tests.Span.Reservation;

using Acme.Workforce.Orgdesign.Span.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class CappedSpanDigestStoreChecks
{
    public static void MemoryCappedSpanDigestStoreFindsWhatItSaved()
    {
        var store = CappedSpanDigestStore.NewMemoryCappedSpanDigestStore();
        var item = CappedSpanDigest.NewCappedSpanDigest("code-649", "title-4");
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
        MemoryCappedSpanDigestStoreFindsWhatItSaved();
    }
}
