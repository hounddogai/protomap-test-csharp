namespace Acme.Workforce.Payrollops.Tests.Garnishment.Target;

using Acme.Workforce.Payrollops.Garnishment.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class GarnishmentSegmentStoreChecks
{
    public static void MemoryGarnishmentSegmentStoreFindsWhatItSaved()
    {
        var store = GarnishmentSegmentStore.NewMemoryGarnishmentSegmentStore();
        var item = GarnishmentSegment.NewGarnishmentSegment("code-56", "label-94");
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
        MemoryGarnishmentSegmentStoreFindsWhatItSaved();
    }
}
