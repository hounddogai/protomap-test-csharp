namespace Acme.Workforce.Talent.Tests.CareerPath.Ledger;

using Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class CareerPathScheduleStoreChecks
{
    public static void MemoryCareerPathScheduleStoreFindsWhatItSaved()
    {
        var store = CareerPathScheduleStore.NewMemoryCareerPathScheduleStore();
        var item = CareerPathSchedule.NewCareerPathSchedule("code-56", "label-427");
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
        MemoryCareerPathScheduleStoreFindsWhatItSaved();
    }
}
