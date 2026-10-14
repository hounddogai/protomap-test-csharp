namespace Acme.Workforce.Absence.Tests.Holiday.Milestone;

using Acme.Workforce.Absence.Holiday.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class HolidayTemplateChecks
{
    public static void HolidayTemplateStartsInOpen()
    {
        var item = HolidayTemplate.NewHolidayTemplate("code-459", "title-800");
        Check.Equal(item.Status, HolidayTemplateStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayTemplateMovesToArchived()
    {
        var item = HolidayTemplate.NewHolidayTemplate("code-459", "title-800");
        try
        {
            item.MoveTo(HolidayTemplateStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayTemplateStatus.Archived, "status");
    }

    public static void HolidayTemplateRejectsMovesFromArchived()
    {
        var item = HolidayTemplate.NewHolidayTemplate("code-459", "title-800");
        item.Status = HolidayTemplateStatus.Archived;
        Check.Fails(() => item.MoveTo(HolidayTemplateStatus.Open), "move");
    }

    public static void RunAll()
    {
        HolidayTemplateStartsInOpen();
        HolidayTemplateMovesToArchived();
        HolidayTemplateRejectsMovesFromArchived();
    }
}
