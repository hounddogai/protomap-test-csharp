namespace Acme.Workforce.Absence.Tests.Policy.Threshold;

using Acme.Workforce.Absence.Policy.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LenientPolicyThresholdStoreChecks
{
    public static void MemoryLenientPolicyThresholdStoreFindsWhatItSaved()
    {
        var store = LenientPolicyThresholdStore.NewMemoryLenientPolicyThresholdStore();
        var item = LenientPolicyThreshold.NewLenientPolicyThreshold("code-693", "name-137");
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
        MemoryLenientPolicyThresholdStoreFindsWhatItSaved();
    }
}
