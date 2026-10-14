namespace Acme.Workforce.Talent.Tests.CareerPath.Digest;

using Acme.Workforce.Talent.CareerPath.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class CareerPathCheckpointStoreChecks
{
    public static void MemoryCareerPathCheckpointStoreFindsWhatItSaved()
    {
        var store = CareerPathCheckpointStore.NewMemoryCareerPathCheckpointStore();
        var item = CareerPathCheckpoint.NewCareerPathCheckpoint("code-395", "label-730");
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
        MemoryCareerPathCheckpointStoreFindsWhatItSaved();
    }
}
