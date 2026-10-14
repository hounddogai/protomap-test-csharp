namespace Acme.Workforce.Talent.Tests.Nomination.Adjustment;

using Acme.Workforce.Talent.Nomination.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SeasonalNominationLedgerStoreChecks
{
    public static void MemorySeasonalNominationLedgerStoreFindsWhatItSaved()
    {
        var store = SeasonalNominationLedgerStore.NewMemorySeasonalNominationLedgerStore();
        var item = SeasonalNominationLedger.NewSeasonalNominationLedger("code-946", "label-251");
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
        MemorySeasonalNominationLedgerStoreFindsWhatItSaved();
    }
}
