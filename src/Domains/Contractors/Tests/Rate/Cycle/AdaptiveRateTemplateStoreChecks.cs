namespace Acme.Workforce.Contractors.Tests.Rate.Cycle;

using Acme.Workforce.Contractors.Rate.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AdaptiveRateTemplateStoreChecks
{
    public static void MemoryAdaptiveRateTemplateStoreFindsWhatItSaved()
    {
        var store = AdaptiveRateTemplateStore.NewMemoryAdaptiveRateTemplateStore();
        var item = AdaptiveRateTemplate.NewAdaptiveRateTemplate("code-707", "label-144");
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
        MemoryAdaptiveRateTemplateStoreFindsWhatItSaved();
    }
}
