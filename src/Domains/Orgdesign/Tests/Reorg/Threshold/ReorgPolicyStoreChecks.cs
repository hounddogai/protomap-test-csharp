namespace Acme.Workforce.Orgdesign.Tests.Reorg.Threshold;

using Acme.Workforce.Orgdesign.Reorg.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ReorgPolicyStoreChecks
{
    public static void MemoryReorgPolicyStoreFindsWhatItSaved()
    {
        var store = ReorgPolicyStore.NewMemoryReorgPolicyStore();
        var item = ReorgPolicy.NewReorgPolicy("code-883", "label-212");
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
        MemoryReorgPolicyStoreFindsWhatItSaved();
    }
}
