namespace Acme.Workforce.Absence.Tests.ReturnPlan.Agreement;

using Acme.Workforce.Absence.ReturnPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class ReturnPlanAgreementStoreChecks
{
    public static void MemoryReturnPlanAgreementStoreFindsWhatItSaved()
    {
        var store = ReturnPlanAgreementStore.NewMemoryReturnPlanAgreementStore();
        var item = ReturnPlanAgreement.NewReturnPlanAgreement("code-255", "label-682");
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
        MemoryReturnPlanAgreementStoreFindsWhatItSaved();
    }
}
