namespace Acme.Workforce.Payrollops.Tests.Deduction.Proposal;

using Acme.Workforce.Payrollops.Deduction.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeductionTemplateStoreChecks
{
    public static void MemoryDeductionTemplateStoreFindsWhatItSaved()
    {
        var store = DeductionTemplateStore.NewMemoryDeductionTemplateStore();
        var item = DeductionTemplate.NewDeductionTemplate("code-71", "label-787");
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
        MemoryDeductionTemplateStoreFindsWhatItSaved();
    }
}
