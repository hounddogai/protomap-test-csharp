namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatAgreement;

using Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

/// <summary>Checks of seat agreement code, run by the project's runner.</summary>
public static class AdaptiveLicenseSeatAgreementStoreChecks
{
    public static void MemoryAdaptiveLicenseSeatAgreementStoreFindsWhatItSaved()
    {
        var store = AdaptiveLicenseSeatAgreementStore.NewMemoryAdaptiveLicenseSeatAgreementStore();
        var item = AdaptiveLicenseSeatAgreement.NewAdaptiveLicenseSeatAgreement("code-384", "label-132");
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
        MemoryAdaptiveLicenseSeatAgreementStoreFindsWhatItSaved();
    }
}
