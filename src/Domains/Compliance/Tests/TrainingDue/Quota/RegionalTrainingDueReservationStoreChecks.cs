namespace Acme.Workforce.Compliance.Tests.TrainingDue.Quota;

using Acme.Workforce.Compliance.TrainingDue.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RegionalTrainingDueReservationStoreChecks
{
    public static void MemoryRegionalTrainingDueReservationStoreFindsWhatItSaved()
    {
        var store = RegionalTrainingDueReservationStore.NewMemoryRegionalTrainingDueReservationStore();
        var item = RegionalTrainingDueReservation.NewRegionalTrainingDueReservation("code-868", "name-936");
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
        MemoryRegionalTrainingDueReservationStoreFindsWhatItSaved();
    }
}
