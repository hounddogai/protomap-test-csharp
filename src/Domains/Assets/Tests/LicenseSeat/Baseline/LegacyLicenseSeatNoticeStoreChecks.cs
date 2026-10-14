namespace Acme.Workforce.Assets.Tests.LicenseSeat.Baseline;

using Acme.Workforce.Assets.LicenseSeat.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LegacyLicenseSeatNoticeStoreChecks
{
    public static void MemoryLegacyLicenseSeatNoticeStoreFindsWhatItSaved()
    {
        var store = LegacyLicenseSeatNoticeStore.NewMemoryLegacyLicenseSeatNoticeStore();
        var item = LegacyLicenseSeatNotice.NewLegacyLicenseSeatNotice("code-890", "label-837");
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
        MemoryLegacyLicenseSeatNoticeStoreFindsWhatItSaved();
    }
}
