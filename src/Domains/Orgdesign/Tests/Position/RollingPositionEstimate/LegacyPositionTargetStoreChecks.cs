namespace Acme.Workforce.Orgdesign.Tests.Position.RollingPositionEstimate;

using Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

/// <summary>Checks of rolling position estimate code, run by the project's runner.</summary>
public static class LegacyPositionTargetStoreChecks
{
    public static void MemoryLegacyPositionTargetStoreFindsWhatItSaved()
    {
        var store = LegacyPositionTargetStore.NewMemoryLegacyPositionTargetStore();
        var item = LegacyPositionTarget.NewLegacyPositionTarget("code-666", "title-561");
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
        MemoryLegacyPositionTargetStoreFindsWhatItSaved();
    }
}
