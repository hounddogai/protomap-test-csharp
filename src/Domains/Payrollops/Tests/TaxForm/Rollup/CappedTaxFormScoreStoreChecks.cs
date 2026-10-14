namespace Acme.Workforce.Payrollops.Tests.TaxForm.Rollup;

using Acme.Workforce.Payrollops.TaxForm.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class CappedTaxFormScoreStoreChecks
{
    public static void MemoryCappedTaxFormScoreStoreFindsWhatItSaved()
    {
        var store = CappedTaxFormScoreStore.NewMemoryCappedTaxFormScoreStore();
        var item = CappedTaxFormScore.NewCappedTaxFormScore("code-376", "title-645");
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
        MemoryCappedTaxFormScoreStoreFindsWhatItSaved();
    }
}
