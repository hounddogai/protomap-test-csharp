namespace Acme.Workforce.Compensation.Tests.EquityGrant.Adjustment;

using Acme.Workforce.Compensation.EquityGrant.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class EquityGrantRequestStoreChecks
{
    public static void MemoryEquityGrantRequestStoreFindsWhatItSaved()
    {
        var store = EquityGrantRequestStore.NewMemoryEquityGrantRequestStore();
        var item = EquityGrantRequest.NewEquityGrantRequest("code-477", "title-792");
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
        MemoryEquityGrantRequestStoreFindsWhatItSaved();
    }
}
