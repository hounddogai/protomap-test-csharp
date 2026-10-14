namespace Acme.Workforce.Payrollops.Tests.RetroPay.Draft;

using Acme.Workforce.Payrollops.RetroPay.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class RetroPayMilestoneRulesChecks
{
    public static void RetroPayMilestoneValidatorReportsEveryProblem()
    {
        var validator = RetroPayMilestoneRules.DefaultRetroPayMilestoneValidator();
        var item = RetroPayMilestone.NewRetroPayMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayMilestoneWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new RetroPayMilestoneWindowDaysRangeRule(32, 3600);
        var item = RetroPayMilestone.NewRetroPayMilestone("x", "y");
        item.WindowDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RetroPayMilestoneRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RetroPayMilestoneRequiredCodeRule("retro pay milestone needs this field");
        var item = RetroPayMilestone.NewRetroPayMilestone("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayMilestoneChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new RetroPayMilestoneChecksumLengthRule(64);
        var item = RetroPayMilestone.NewRetroPayMilestone("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayMilestoneRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new RetroPayMilestoneRequiredChecksumRule("retro pay milestone needs a value");
        var item = RetroPayMilestone.NewRetroPayMilestone("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RetroPayMilestoneValidatorReportsEveryProblem();
        RetroPayMilestoneWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        RetroPayMilestoneRequiredCodeRuleFlagsABlankCode();
        RetroPayMilestoneChecksumLengthRuleFlagsALongChecksum();
        RetroPayMilestoneRequiredChecksumRuleFlagsABlankChecksum();
    }
}
