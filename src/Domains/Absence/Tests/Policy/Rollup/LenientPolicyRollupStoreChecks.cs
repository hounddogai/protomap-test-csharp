namespace Acme.Workforce.Absence.Tests.Policy.Rollup;

using Acme.Workforce.Absence.Policy.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LenientPolicyRollupStoreChecks
{
    public static void MemoryLenientPolicyRollupStoreFindsWhatItSaved()
    {
        var store = LenientPolicyRollupStore.NewMemoryLenientPolicyRollupStore();
        var item = LenientPolicyRollup.NewLenientPolicyRollup("code-773", "name-570");
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
        MemoryLenientPolicyRollupStoreFindsWhatItSaved();
    }
}
