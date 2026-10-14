namespace Acme.Workforce.Payrollops.Tests.PayRun.Checkpoint;

using Acme.Workforce.Payrollops.PayRun.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class PayRunTemplateStoreChecks
{
    public static void MemoryPayRunTemplateStoreFindsWhatItSaved()
    {
        var store = PayRunTemplateStore.NewMemoryPayRunTemplateStore();
        var item = PayRunTemplate.NewPayRunTemplate("code-809", "label-301");
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
        MemoryPayRunTemplateStoreFindsWhatItSaved();
    }
}
