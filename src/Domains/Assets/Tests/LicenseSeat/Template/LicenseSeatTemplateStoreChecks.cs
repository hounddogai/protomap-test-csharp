namespace Acme.Workforce.Assets.Tests.LicenseSeat.Template;

using Acme.Workforce.Assets.LicenseSeat.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LicenseSeatTemplateStoreChecks
{
    public static void MemoryLicenseSeatTemplateStoreFindsWhatItSaved()
    {
        var store = LicenseSeatTemplateStore.NewMemoryLicenseSeatTemplateStore();
        var item = LicenseSeatTemplate.NewLicenseSeatTemplate("code-558", "name-666");
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
        MemoryLicenseSeatTemplateStoreFindsWhatItSaved();
    }
}
