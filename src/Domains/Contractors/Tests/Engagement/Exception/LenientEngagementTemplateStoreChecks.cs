namespace Acme.Workforce.Contractors.Tests.Engagement.Exception;

using Acme.Workforce.Contractors.Engagement.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class LenientEngagementTemplateStoreChecks
{
    public static void MemoryLenientEngagementTemplateStoreFindsWhatItSaved()
    {
        var store = LenientEngagementTemplateStore.NewMemoryLenientEngagementTemplateStore();
        var item = LenientEngagementTemplate.NewLenientEngagementTemplate("code-863", "label-667");
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
        MemoryLenientEngagementTemplateStoreFindsWhatItSaved();
    }
}
