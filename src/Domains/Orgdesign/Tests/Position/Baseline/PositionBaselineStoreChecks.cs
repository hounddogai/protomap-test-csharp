namespace Acme.Workforce.Orgdesign.Tests.Position.Baseline;

using Acme.Workforce.Orgdesign.Position.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PositionBaselineStoreChecks
{
    public static void MemoryPositionBaselineStoreFindsWhatItSaved()
    {
        var store = PositionBaselineStore.NewMemoryPositionBaselineStore();
        var item = PositionBaseline.NewPositionBaseline("code-945", "label-153");
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
        MemoryPositionBaselineStoreFindsWhatItSaved();
    }
}
