namespace Acme.Workforce.Absence.Tests.Holiday.Milestone;

using Acme.Workforce.Absence.Holiday.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class HolidayTemplateStoreChecks
{
    public static void MemoryHolidayTemplateStoreFindsWhatItSaved()
    {
        var store = HolidayTemplateStore.NewMemoryHolidayTemplateStore();
        var item = HolidayTemplate.NewHolidayTemplate("code-459", "title-800");
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
        MemoryHolidayTemplateStoreFindsWhatItSaved();
    }
}
