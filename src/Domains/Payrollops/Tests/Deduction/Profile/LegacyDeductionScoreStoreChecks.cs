namespace Acme.Workforce.Payrollops.Tests.Deduction.Profile;

using Acme.Workforce.Payrollops.Deduction.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class LegacyDeductionScoreStoreChecks
{
    public static void MemoryLegacyDeductionScoreStoreFindsWhatItSaved()
    {
        var store = LegacyDeductionScoreStore.NewMemoryLegacyDeductionScoreStore();
        var item = LegacyDeductionScore.NewLegacyDeductionScore("code-820", "name-320");
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
        MemoryLegacyDeductionScoreStoreFindsWhatItSaved();
    }
}
