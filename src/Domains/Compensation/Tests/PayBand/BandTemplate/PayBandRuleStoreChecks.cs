namespace Acme.Workforce.Compensation.Tests.PayBand.BandTemplate;

using Acme.Workforce.Compensation.PayBand.BandTemplate;

/// <summary>Checks of band template code, run by the project's runner.</summary>
public static class PayBandRuleStoreChecks
{
    public static void MemoryPayBandRuleStoreFindsWhatItSaved()
    {
        var store = PayBandRuleStore.NewMemoryPayBandRuleStore();
        var item = PayBandRule.NewPayBandRule("code-811", "label-612");
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
        MemoryPayBandRuleStoreFindsWhatItSaved();
    }
}
