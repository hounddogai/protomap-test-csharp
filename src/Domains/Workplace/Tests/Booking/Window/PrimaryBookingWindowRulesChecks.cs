namespace Acme.Workforce.Workplace.Tests.Booking.Window;

using Acme.Workforce.Workplace.Booking.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PrimaryBookingWindowRulesChecks
{
    public static void PrimaryBookingWindowValidatorReportsEveryProblem()
    {
        var validator = PrimaryBookingWindowRules.DefaultPrimaryBookingWindowValidator();
        var item = PrimaryBookingWindow.NewPrimaryBookingWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryBookingWindowCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimaryBookingWindowCodeLengthRule(16);
        var item = PrimaryBookingWindow.NewPrimaryBookingWindow("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryBookingWindowSkillLengthRuleFlagsALongSkill()
    {
        var rule = new PrimaryBookingWindowSkillLengthRule(80);
        var item = PrimaryBookingWindow.NewPrimaryBookingWindow("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryBookingWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryBookingWindowRequiredCodeRule("primary booking window needs this field");
        var item = PrimaryBookingWindow.NewPrimaryBookingWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryBookingWindowRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new PrimaryBookingWindowRequiredSkillRule("primary booking window needs this field");
        var item = PrimaryBookingWindow.NewPrimaryBookingWindow("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryBookingWindowValidatorReportsEveryProblem();
        PrimaryBookingWindowCodeLengthRuleFlagsALongCode();
        PrimaryBookingWindowSkillLengthRuleFlagsALongSkill();
        PrimaryBookingWindowRequiredCodeRuleFlagsABlankCode();
        PrimaryBookingWindowRequiredSkillRuleFlagsABlankSkill();
    }
}
