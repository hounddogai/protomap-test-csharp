namespace Acme.Workforce.Absence.Tests.Holiday.Metric;

using Acme.Workforce.Absence.Holiday.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class HolidayAgreementStoreChecks
{
    public static void MemoryHolidayAgreementStoreFindsWhatItSaved()
    {
        var store = HolidayAgreementStore.NewMemoryHolidayAgreementStore();
        var item = HolidayAgreement.NewHolidayAgreement("code-290", "name-447");
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
        MemoryHolidayAgreementStoreFindsWhatItSaved();
    }
}
