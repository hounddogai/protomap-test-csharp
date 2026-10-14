namespace Acme.Workforce.Payrollops.Tests.Payslip.Snapshot;

using Acme.Workforce.Payrollops.Payslip.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class PayslipSnapshotStoreChecks
{
    public static void MemoryPayslipSnapshotStoreFindsWhatItSaved()
    {
        var store = PayslipSnapshotStore.NewMemoryPayslipSnapshotStore();
        var item = PayslipSnapshot.NewPayslipSnapshot("code-677", "label-667");
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
        MemoryPayslipSnapshotStoreFindsWhatItSaved();
    }
}
