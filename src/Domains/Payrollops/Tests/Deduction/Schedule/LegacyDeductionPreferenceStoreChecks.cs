namespace Acme.Workforce.Payrollops.Tests.Deduction.Schedule;

using Acme.Workforce.Payrollops.Deduction.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LegacyDeductionPreferenceStoreChecks
{
    public static void MemoryLegacyDeductionPreferenceStoreFindsWhatItSaved()
    {
        var store = LegacyDeductionPreferenceStore.NewMemoryLegacyDeductionPreferenceStore();
        var item = LegacyDeductionPreference.NewLegacyDeductionPreference("code-315", "name-270");
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
        MemoryLegacyDeductionPreferenceStoreFindsWhatItSaved();
    }
}
