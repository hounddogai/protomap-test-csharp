namespace Acme.Workforce.Payrollops.Tests.Payslip.Checkpoint;

using Acme.Workforce.Payrollops.Payslip.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class GlobalPayslipCheckpointStoreChecks
{
    public static void MemoryGlobalPayslipCheckpointStoreFindsWhatItSaved()
    {
        var store = GlobalPayslipCheckpointStore.NewMemoryGlobalPayslipCheckpointStore();
        var item = GlobalPayslipCheckpoint.NewGlobalPayslipCheckpoint("code-107", "name-675");
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
        MemoryGlobalPayslipCheckpointStoreFindsWhatItSaved();
    }
}
