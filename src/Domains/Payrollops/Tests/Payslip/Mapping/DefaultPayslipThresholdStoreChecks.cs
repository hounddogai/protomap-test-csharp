namespace Acme.Workforce.Payrollops.Tests.Payslip.Mapping;

using Acme.Workforce.Payrollops.Payslip.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DefaultPayslipThresholdStoreChecks
{
    public static void MemoryDefaultPayslipThresholdStoreFindsWhatItSaved()
    {
        var store = DefaultPayslipThresholdStore.NewMemoryDefaultPayslipThresholdStore();
        var item = DefaultPayslipThreshold.NewDefaultPayslipThreshold("code-36", "title-893");
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
        MemoryDefaultPayslipThresholdStoreFindsWhatItSaved();
    }
}
