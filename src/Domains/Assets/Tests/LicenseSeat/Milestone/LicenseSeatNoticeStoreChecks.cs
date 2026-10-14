namespace Acme.Workforce.Assets.Tests.LicenseSeat.Milestone;

using Acme.Workforce.Assets.LicenseSeat.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LicenseSeatNoticeStoreChecks
{
    public static void MemoryLicenseSeatNoticeStoreFindsWhatItSaved()
    {
        var store = LicenseSeatNoticeStore.NewMemoryLicenseSeatNoticeStore();
        var item = LicenseSeatNotice.NewLicenseSeatNotice("code-941", "title-40");
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
        MemoryLicenseSeatNoticeStoreFindsWhatItSaved();
    }
}
