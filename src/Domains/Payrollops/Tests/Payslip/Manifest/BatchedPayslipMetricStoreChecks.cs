namespace Acme.Workforce.Payrollops.Tests.Payslip.Manifest;

using Acme.Workforce.Payrollops.Payslip.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class BatchedPayslipMetricStoreChecks
{
    public static void MemoryBatchedPayslipMetricStoreFindsWhatItSaved()
    {
        var store = BatchedPayslipMetricStore.NewMemoryBatchedPayslipMetricStore();
        var item = BatchedPayslipMetric.NewBatchedPayslipMetric("code-639", "title-10");
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
        MemoryBatchedPayslipMetricStoreFindsWhatItSaved();
    }
}
