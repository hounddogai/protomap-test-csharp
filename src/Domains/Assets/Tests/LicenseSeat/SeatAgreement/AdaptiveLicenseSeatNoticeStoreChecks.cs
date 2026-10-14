namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatAgreement;

using Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

/// <summary>Checks of seat agreement code, run by the project's runner.</summary>
public static class AdaptiveLicenseSeatNoticeStoreChecks
{
    public static void MemoryAdaptiveLicenseSeatNoticeStoreFindsWhatItSaved()
    {
        var store = AdaptiveLicenseSeatNoticeStore.NewMemoryAdaptiveLicenseSeatNoticeStore();
        var item = AdaptiveLicenseSeatNotice.NewAdaptiveLicenseSeatNotice("code-343", "title-551");
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
        MemoryAdaptiveLicenseSeatNoticeStoreFindsWhatItSaved();
    }
}
