namespace Acme.Workforce.Absence.Tests.Policy.PolicyRollup;

using Acme.Workforce.Absence.Policy.PolicyRollup;

/// <summary>Checks of policy rollup code, run by the project's runner.</summary>
public static class PrimaryPolicyRollupStoreChecks
{
    public static void MemoryPrimaryPolicyRollupStoreFindsWhatItSaved()
    {
        var store = PrimaryPolicyRollupStore.NewMemoryPrimaryPolicyRollupStore();
        var item = PrimaryPolicyRollup.NewPrimaryPolicyRollup("code-999", "label-331");
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
        MemoryPrimaryPolicyRollupStoreFindsWhatItSaved();
    }
}
