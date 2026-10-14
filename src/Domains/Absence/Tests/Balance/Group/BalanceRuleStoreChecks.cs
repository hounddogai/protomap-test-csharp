namespace Acme.Workforce.Absence.Tests.Balance.Group;

using Acme.Workforce.Absence.Balance.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BalanceRuleStoreChecks
{
    public static void MemoryBalanceRuleStoreFindsWhatItSaved()
    {
        var store = BalanceRuleStore.NewMemoryBalanceRuleStore();
        var item = BalanceRule.NewBalanceRule("code-156", "title-590");
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
        MemoryBalanceRuleStoreFindsWhatItSaved();
    }
}
