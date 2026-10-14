namespace Acme.Workforce.Absence.Tests.Accrual.Template;

using Acme.Workforce.Absence.Accrual.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class SeasonalAccrualTemplateStoreChecks
{
    public static void MemorySeasonalAccrualTemplateStoreFindsWhatItSaved()
    {
        var store = SeasonalAccrualTemplateStore.NewMemorySeasonalAccrualTemplateStore();
        var item = SeasonalAccrualTemplate.NewSeasonalAccrualTemplate("code-552", "name-482");
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
        MemorySeasonalAccrualTemplateStoreFindsWhatItSaved();
    }
}
