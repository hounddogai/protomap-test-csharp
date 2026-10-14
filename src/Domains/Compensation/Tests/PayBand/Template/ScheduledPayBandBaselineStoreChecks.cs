namespace Acme.Workforce.Compensation.Tests.PayBand.Template;

using Acme.Workforce.Compensation.PayBand.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ScheduledPayBandBaselineStoreChecks
{
    public static void MemoryScheduledPayBandBaselineStoreFindsWhatItSaved()
    {
        var store = ScheduledPayBandBaselineStore.NewMemoryScheduledPayBandBaselineStore();
        var item = ScheduledPayBandBaseline.NewScheduledPayBandBaseline("code-669", "title-544");
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
        MemoryScheduledPayBandBaselineStoreFindsWhatItSaved();
    }
}
