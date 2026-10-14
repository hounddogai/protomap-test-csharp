namespace Acme.Workforce.Payrollops.Tests.TaxForm.Score;

using Acme.Workforce.Payrollops.TaxForm.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class AdaptiveTaxFormScoreStoreChecks
{
    public static void MemoryAdaptiveTaxFormScoreStoreFindsWhatItSaved()
    {
        var store = AdaptiveTaxFormScoreStore.NewMemoryAdaptiveTaxFormScoreStore();
        var item = AdaptiveTaxFormScore.NewAdaptiveTaxFormScore("code-152", "name-68");
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
        MemoryAdaptiveTaxFormScoreStoreFindsWhatItSaved();
    }
}
