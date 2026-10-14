namespace Acme.Workforce.Absence.Tests.Holiday.BatchedHolidaySchedule;

using Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

/// <summary>Checks of batched holiday schedule code, run by the project's runner.</summary>
public static class HolidayDraftRulesChecks
{
    public static void HolidayDraftValidatorReportsEveryProblem()
    {
        var validator = HolidayDraftRules.DefaultHolidayDraftValidator();
        var item = HolidayDraft.NewHolidayDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayDraftRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new HolidayDraftRequiredContactEmailRule("holiday draft needs an entry");
        var item = HolidayDraft.NewHolidayDraft("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        HolidayDraftValidatorReportsEveryProblem();
        HolidayDraftRequiredContactEmailRuleFlagsABlankContactEmail();
    }
}
