namespace Acme.Workforce.Workplace.Tests.FloorPlan.Metric;

using Acme.Workforce.Workplace.FloorPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CappedFloorPlanAgreementStoreChecks
{
    public static void MemoryCappedFloorPlanAgreementStoreFindsWhatItSaved()
    {
        var store = CappedFloorPlanAgreementStore.NewMemoryCappedFloorPlanAgreementStore();
        var item = CappedFloorPlanAgreement.NewCappedFloorPlanAgreement("code-690", "name-895");
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
        MemoryCappedFloorPlanAgreementStoreFindsWhatItSaved();
    }
}
