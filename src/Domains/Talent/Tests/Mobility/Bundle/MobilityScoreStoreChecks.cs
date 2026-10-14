namespace Acme.Workforce.Talent.Tests.Mobility.Bundle;

using Acme.Workforce.Talent.Mobility.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class MobilityScoreStoreChecks
{
    public static void MemoryMobilityScoreStoreFindsWhatItSaved()
    {
        var store = MobilityScoreStore.NewMemoryMobilityScoreStore();
        var item = MobilityScore.NewMobilityScore("code-138", "title-731");
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
        MemoryMobilityScoreStoreFindsWhatItSaved();
    }
}
