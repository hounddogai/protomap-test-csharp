namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingSegment;

using Acme.Workforce.Contractors.Offboarding.OffboardingSegment;

/// <summary>Checks of offboarding segment code, run by the project's runner.</summary>
public static class AdaptiveOffboardingSegmentStoreChecks
{
    public static void MemoryAdaptiveOffboardingSegmentStoreFindsWhatItSaved()
    {
        var store = AdaptiveOffboardingSegmentStore.NewMemoryAdaptiveOffboardingSegmentStore();
        var item = AdaptiveOffboardingSegment.NewAdaptiveOffboardingSegment("code-108", "title-309");
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
        MemoryAdaptiveOffboardingSegmentStoreFindsWhatItSaved();
    }
}
