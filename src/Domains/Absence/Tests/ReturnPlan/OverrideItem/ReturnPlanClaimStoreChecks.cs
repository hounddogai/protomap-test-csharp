namespace Acme.Workforce.Absence.Tests.ReturnPlan.OverrideItem;

using Acme.Workforce.Absence.ReturnPlan.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class ReturnPlanClaimStoreChecks
{
    public static void MemoryReturnPlanClaimStoreFindsWhatItSaved()
    {
        var store = ReturnPlanClaimStore.NewMemoryReturnPlanClaimStore();
        var item = ReturnPlanClaim.NewReturnPlanClaim("code-65", "title-81");
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
        MemoryReturnPlanClaimStoreFindsWhatItSaved();
    }
}
