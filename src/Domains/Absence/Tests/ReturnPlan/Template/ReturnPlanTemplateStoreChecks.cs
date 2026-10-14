namespace Acme.Workforce.Absence.Tests.ReturnPlan.Template;

using Acme.Workforce.Absence.ReturnPlan.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ReturnPlanTemplateStoreChecks
{
    public static void MemoryReturnPlanTemplateStoreFindsWhatItSaved()
    {
        var store = ReturnPlanTemplateStore.NewMemoryReturnPlanTemplateStore();
        var item = ReturnPlanTemplate.NewReturnPlanTemplate("code-731", "label-35");
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
        MemoryReturnPlanTemplateStoreFindsWhatItSaved();
    }
}
