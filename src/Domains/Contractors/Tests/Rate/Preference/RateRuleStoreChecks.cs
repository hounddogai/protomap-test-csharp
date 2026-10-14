namespace Acme.Workforce.Contractors.Tests.Rate.Preference;

using Acme.Workforce.Contractors.Rate.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RateRuleStoreChecks
{
    public static void MemoryRateRuleStoreFindsWhatItSaved()
    {
        var store = RateRuleStore.NewMemoryRateRuleStore();
        var item = RateRule.NewRateRule("code-87", "name-441");
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
        MemoryRateRuleStoreFindsWhatItSaved();
    }
}
