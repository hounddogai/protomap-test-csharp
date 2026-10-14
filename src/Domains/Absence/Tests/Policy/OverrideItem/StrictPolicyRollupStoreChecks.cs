namespace Acme.Workforce.Absence.Tests.Policy.OverrideItem;

using Acme.Workforce.Absence.Policy.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class StrictPolicyRollupStoreChecks
{
    public static void MemoryStrictPolicyRollupStoreFindsWhatItSaved()
    {
        var store = StrictPolicyRollupStore.NewMemoryStrictPolicyRollupStore();
        var item = StrictPolicyRollup.NewStrictPolicyRollup("code-53", "label-829");
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
        MemoryStrictPolicyRollupStoreFindsWhatItSaved();
    }
}
