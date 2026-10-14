namespace Acme.Workforce.Payrollops.Tests.PayRun.RunLedger;

using Acme.Workforce.Payrollops.PayRun.RunLedger;

/// <summary>Checks of run ledger code, run by the project's runner.</summary>
public static class PayRunPreferenceStoreChecks
{
    public static void MemoryPayRunPreferenceStoreFindsWhatItSaved()
    {
        var store = PayRunPreferenceStore.NewMemoryPayRunPreferenceStore();
        var item = PayRunPreference.NewPayRunPreference("code-50", "title-435");
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
        MemoryPayRunPreferenceStoreFindsWhatItSaved();
    }
}
