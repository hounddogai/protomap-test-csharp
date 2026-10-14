namespace Acme.Workforce.Orgdesign.Tests.Position.RollingPositionEstimate;

using Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

/// <summary>Checks of rolling position estimate code, run by the project's runner.</summary>
public static class LenientPositionEstimateStoreChecks
{
    public static void MemoryLenientPositionEstimateStoreFindsWhatItSaved()
    {
        var store = LenientPositionEstimateStore.NewMemoryLenientPositionEstimateStore();
        var item = LenientPositionEstimate.NewLenientPositionEstimate("code-148", "name-953");
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
        MemoryLenientPositionEstimateStoreFindsWhatItSaved();
    }
}
