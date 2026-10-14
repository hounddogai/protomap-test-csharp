namespace Acme.Workforce.Compensation.Tests.BonusPool.Allocation;

using Acme.Workforce.Compensation.BonusPool.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class LenientBonusPoolPolicyStoreChecks
{
    public static void MemoryLenientBonusPoolPolicyStoreFindsWhatItSaved()
    {
        var store = LenientBonusPoolPolicyStore.NewMemoryLenientBonusPoolPolicyStore();
        var item = LenientBonusPoolPolicy.NewLenientBonusPoolPolicy("code-372", "name-349");
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
        MemoryLenientBonusPoolPolicyStoreFindsWhatItSaved();
    }
}
