namespace Acme.Workforce.Orgdesign.Tests.Position.RollingPositionEstimate;

using Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

/// <summary>Checks of rolling position estimate code, run by the project's runner.</summary>
public static class LegacyPositionEstimateStoreChecks
{
    public static void MemoryLegacyPositionEstimateStoreFindsWhatItSaved()
    {
        var store = LegacyPositionEstimateStore.NewMemoryLegacyPositionEstimateStore();
        var item = LegacyPositionEstimate.NewLegacyPositionEstimate("code-588", "label-249");
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
        MemoryLegacyPositionEstimateStoreFindsWhatItSaved();
    }
}
