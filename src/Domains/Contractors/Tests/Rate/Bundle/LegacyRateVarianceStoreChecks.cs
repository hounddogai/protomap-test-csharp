namespace Acme.Workforce.Contractors.Tests.Rate.Bundle;

using Acme.Workforce.Contractors.Rate.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LegacyRateVarianceStoreChecks
{
    public static void MemoryLegacyRateVarianceStoreFindsWhatItSaved()
    {
        var store = LegacyRateVarianceStore.NewMemoryLegacyRateVarianceStore();
        var item = LegacyRateVariance.NewLegacyRateVariance("code-635", "name-336");
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
        MemoryLegacyRateVarianceStoreFindsWhatItSaved();
    }
}
