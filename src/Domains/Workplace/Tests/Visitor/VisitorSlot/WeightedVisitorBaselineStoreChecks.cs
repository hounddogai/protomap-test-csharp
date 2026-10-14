namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorSlot;

using Acme.Workforce.Workplace.Visitor.VisitorSlot;

/// <summary>Checks of visitor slot code, run by the project's runner.</summary>
public static class WeightedVisitorBaselineStoreChecks
{
    public static void MemoryWeightedVisitorBaselineStoreFindsWhatItSaved()
    {
        var store = WeightedVisitorBaselineStore.NewMemoryWeightedVisitorBaselineStore();
        var item = WeightedVisitorBaseline.NewWeightedVisitorBaseline("code-890", "label-845");
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
        MemoryWeightedVisitorBaselineStoreFindsWhatItSaved();
    }
}
