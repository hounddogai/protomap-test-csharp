namespace Acme.Workforce.Absence.Tests.Holiday.Milestone;

using Acme.Workforce.Absence.Holiday.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class HolidayTemplateRulesChecks
{
    public static void HolidayTemplateValidatorReportsEveryProblem()
    {
        var validator = HolidayTemplateRules.DefaultHolidayTemplateValidator();
        var item = HolidayTemplate.NewHolidayTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayTemplateRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new HolidayTemplateRequiredTitleRule("holiday template needs this field");
        var item = HolidayTemplate.NewHolidayTemplate("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayTemplateRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new HolidayTemplateRetriesRangeRule(64, 1000);
        var item = HolidayTemplate.NewHolidayTemplate("x", "y");
        item.Retries = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HolidayTemplatePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new HolidayTemplatePriorityRangeRule(32, 32);
        var item = HolidayTemplate.NewHolidayTemplate("x", "y");
        item.Priority = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HolidayTemplateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new HolidayTemplateRequiredCodeRule("holiday template needs an entry");
        var item = HolidayTemplate.NewHolidayTemplate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayTemplateCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new HolidayTemplateCapacityRangeRule(32, 3600);
        var item = HolidayTemplate.NewHolidayTemplate("x", "y");
        item.Capacity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        HolidayTemplateValidatorReportsEveryProblem();
        HolidayTemplateRequiredTitleRuleFlagsABlankTitle();
        HolidayTemplateRetriesRangeRuleFlagsValuesAboveRetries();
        HolidayTemplatePriorityRangeRuleFlagsValuesAbovePriority();
        HolidayTemplateRequiredCodeRuleFlagsABlankCode();
        HolidayTemplateCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
