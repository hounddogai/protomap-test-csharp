namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAgreement;

using Acme.Workforce.Workplace.Occupancy.OccupancyAgreement;

/// <summary>Checks of occupancy agreement code, run by the project's runner.</summary>
public static class AdaptiveOccupancyAgreementStoreChecks
{
    public static void MemoryAdaptiveOccupancyAgreementStoreFindsWhatItSaved()
    {
        var store = AdaptiveOccupancyAgreementStore.NewMemoryAdaptiveOccupancyAgreementStore();
        var item = AdaptiveOccupancyAgreement.NewAdaptiveOccupancyAgreement("code-37", "name-725");
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
        MemoryAdaptiveOccupancyAgreementStoreFindsWhatItSaved();
    }
}
