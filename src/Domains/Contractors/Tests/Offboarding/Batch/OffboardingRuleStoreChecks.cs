namespace Acme.Workforce.Contractors.Tests.Offboarding.Batch;

using Acme.Workforce.Contractors.Offboarding.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class OffboardingRuleStoreChecks
{
    public static void MemoryOffboardingRuleStoreFindsWhatItSaved()
    {
        var store = OffboardingRuleStore.NewMemoryOffboardingRuleStore();
        var item = OffboardingRule.NewOffboardingRule("code-927", "label-508");
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
        MemoryOffboardingRuleStoreFindsWhatItSaved();
    }
}
