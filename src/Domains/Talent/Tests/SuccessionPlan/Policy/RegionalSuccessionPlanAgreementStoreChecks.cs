namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Policy;

using Acme.Workforce.Talent.SuccessionPlan.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class RegionalSuccessionPlanAgreementStoreChecks
{
    public static void MemoryRegionalSuccessionPlanAgreementStoreFindsWhatItSaved()
    {
        var store = RegionalSuccessionPlanAgreementStore.NewMemoryRegionalSuccessionPlanAgreementStore();
        var item = RegionalSuccessionPlanAgreement.NewRegionalSuccessionPlanAgreement("code-421", "label-181");
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
        MemoryRegionalSuccessionPlanAgreementStoreFindsWhatItSaved();
    }
}
