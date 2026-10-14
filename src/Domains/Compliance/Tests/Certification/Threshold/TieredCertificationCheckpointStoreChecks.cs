namespace Acme.Workforce.Compliance.Tests.Certification.Threshold;

using Acme.Workforce.Compliance.Certification.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class TieredCertificationCheckpointStoreChecks
{
    public static void MemoryTieredCertificationCheckpointStoreFindsWhatItSaved()
    {
        var store = TieredCertificationCheckpointStore.NewMemoryTieredCertificationCheckpointStore();
        var item = TieredCertificationCheckpoint.NewTieredCertificationCheckpoint("code-841", "name-481");
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
        MemoryTieredCertificationCheckpointStoreFindsWhatItSaved();
    }
}
