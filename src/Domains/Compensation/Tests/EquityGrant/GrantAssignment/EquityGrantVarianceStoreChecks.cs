namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantAssignment;

using Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

/// <summary>Checks of grant assignment code, run by the project's runner.</summary>
public static class EquityGrantVarianceStoreChecks
{
    public static void MemoryEquityGrantVarianceStoreFindsWhatItSaved()
    {
        var store = EquityGrantVarianceStore.NewMemoryEquityGrantVarianceStore();
        var item = EquityGrantVariance.NewEquityGrantVariance("code-615", "title-587");
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
        MemoryEquityGrantVarianceStoreFindsWhatItSaved();
    }
}
