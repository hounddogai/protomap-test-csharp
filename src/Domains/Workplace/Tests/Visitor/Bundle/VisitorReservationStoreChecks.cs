namespace Acme.Workforce.Workplace.Tests.Visitor.Bundle;

using Acme.Workforce.Workplace.Visitor.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class VisitorReservationStoreChecks
{
    public static void MemoryVisitorReservationStoreFindsWhatItSaved()
    {
        var store = VisitorReservationStore.NewMemoryVisitorReservationStore();
        var item = VisitorReservation.NewVisitorReservation("code-720", "title-944");
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
        MemoryVisitorReservationStoreFindsWhatItSaved();
    }
}
