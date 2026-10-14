namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementTemplate;

using Acme.Workforce.Contractors.Engagement.EngagementTemplate;

/// <summary>Checks of engagement template code, run by the project's runner.</summary>
public static class WeightedEngagementTemplateStoreChecks
{
    public static void MemoryWeightedEngagementTemplateStoreFindsWhatItSaved()
    {
        var store = WeightedEngagementTemplateStore.NewMemoryWeightedEngagementTemplateStore();
        var item = WeightedEngagementTemplate.NewWeightedEngagementTemplate("code-898", "title-370");
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
        MemoryWeightedEngagementTemplateStoreFindsWhatItSaved();
    }
}
