namespace Acme.Workforce.Assets.Tests.LicenseSeat.Assignment;

using Acme.Workforce.Assets.LicenseSeat.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TieredLicenseSeatRuleStoreChecks
{
    public static void MemoryTieredLicenseSeatRuleStoreFindsWhatItSaved()
    {
        var store = TieredLicenseSeatRuleStore.NewMemoryTieredLicenseSeatRuleStore();
        var item = TieredLicenseSeatRule.NewTieredLicenseSeatRule("code-274", "label-138");
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
        MemoryTieredLicenseSeatRuleStoreFindsWhatItSaved();
    }
}
