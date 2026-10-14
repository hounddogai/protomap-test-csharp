namespace Acme.Workforce.Payrollops.Tests.Payslip.Estimate;

using Acme.Workforce.Payrollops.Payslip.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PayslipCheckpointStoreChecks
{
    public static void MemoryPayslipCheckpointStoreFindsWhatItSaved()
    {
        var store = PayslipCheckpointStore.NewMemoryPayslipCheckpointStore();
        var item = PayslipCheckpoint.NewPayslipCheckpoint("code-134", "name-543");
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
        MemoryPayslipCheckpointStoreFindsWhatItSaved();
    }
}
