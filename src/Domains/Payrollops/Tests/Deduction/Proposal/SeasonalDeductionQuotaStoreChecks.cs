namespace Acme.Workforce.Payrollops.Tests.Deduction.Proposal;

using Acme.Workforce.Payrollops.Deduction.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SeasonalDeductionQuotaStoreChecks
{
    public static void MemorySeasonalDeductionQuotaStoreFindsWhatItSaved()
    {
        var store = SeasonalDeductionQuotaStore.NewMemorySeasonalDeductionQuotaStore();
        var item = SeasonalDeductionQuota.NewSeasonalDeductionQuota("code-242", "label-335");
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
        MemorySeasonalDeductionQuotaStoreFindsWhatItSaved();
    }
}
