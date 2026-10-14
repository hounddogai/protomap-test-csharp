namespace Acme.Workforce.Orgdesign.Tests.Span.Exception;

using Acme.Workforce.Orgdesign.Span.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class SpanReservationStoreChecks
{
    public static void MemorySpanReservationStoreFindsWhatItSaved()
    {
        var store = SpanReservationStore.NewMemorySpanReservationStore();
        var item = SpanReservation.NewSpanReservation("code-219", "title-253");
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
        MemorySpanReservationStoreFindsWhatItSaved();
    }
}
