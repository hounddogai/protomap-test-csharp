namespace Acme.Workforce.Workplace.Tests.Visitor.Draft;

using Acme.Workforce.Workplace.Visitor.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientVisitorReservationStoreChecks
{
    public static void MemoryLenientVisitorReservationStoreFindsWhatItSaved()
    {
        var store = LenientVisitorReservationStore.NewMemoryLenientVisitorReservationStore();
        var item = LenientVisitorReservation.NewLenientVisitorReservation("code-937", "name-953");
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
        MemoryLenientVisitorReservationStoreFindsWhatItSaved();
    }
}
