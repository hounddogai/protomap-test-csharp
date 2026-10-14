namespace Acme.Workforce.Payrollops.Tests.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

/// <summary>Checks of rolling payslip snapshot code, run by the project's runner.</summary>
public static class PrimaryPayslipSnapshotStoreChecks
{
    public static void MemoryPrimaryPayslipSnapshotStoreFindsWhatItSaved()
    {
        var store = PrimaryPayslipSnapshotStore.NewMemoryPrimaryPayslipSnapshotStore();
        var item = PrimaryPayslipSnapshot.NewPrimaryPayslipSnapshot("code-995", "label-152");
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
        MemoryPrimaryPayslipSnapshotStoreFindsWhatItSaved();
    }
}
