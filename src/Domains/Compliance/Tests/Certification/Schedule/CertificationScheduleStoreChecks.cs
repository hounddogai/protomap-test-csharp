namespace Acme.Workforce.Compliance.Tests.Certification.Schedule;

using Acme.Workforce.Compliance.Certification.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class CertificationScheduleStoreChecks
{
    public static void MemoryCertificationScheduleStoreFindsWhatItSaved()
    {
        var store = CertificationScheduleStore.NewMemoryCertificationScheduleStore();
        var item = CertificationSchedule.NewCertificationSchedule("code-671", "title-311");
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
        MemoryCertificationScheduleStoreFindsWhatItSaved();
    }
}
