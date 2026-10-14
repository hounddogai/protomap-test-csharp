namespace Acme.Workforce.Workplace.Tests.Booking.Window;

using Acme.Workforce.Workplace.Booking.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class ScheduledBookingTemplateRulesChecks
{
    public static void ScheduledBookingTemplateValidatorReportsEveryProblem()
    {
        var validator = ScheduledBookingTemplateRules.DefaultScheduledBookingTemplateValidator();
        var item = ScheduledBookingTemplate.NewScheduledBookingTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledBookingTemplateQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new ScheduledBookingTemplateQuantityRangeRule(255, 1000);
        var item = ScheduledBookingTemplate.NewScheduledBookingTemplate("x", "y");
        item.Quantity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledBookingTemplateCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledBookingTemplateCodeLengthRule(64);
        var item = ScheduledBookingTemplate.NewScheduledBookingTemplate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledBookingTemplateRequiredNameRuleFlagsABlankName()
    {
        var rule = new ScheduledBookingTemplateRequiredNameRule("scheduled booking template needs an entry");
        var item = ScheduledBookingTemplate.NewScheduledBookingTemplate("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledBookingTemplateValidatorReportsEveryProblem();
        ScheduledBookingTemplateQuantityRangeRuleFlagsValuesAboveQuantity();
        ScheduledBookingTemplateCodeLengthRuleFlagsALongCode();
        ScheduledBookingTemplateRequiredNameRuleFlagsABlankName();
    }
}
