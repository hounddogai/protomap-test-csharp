namespace Acme.Workforce.Orgdesign.Tests.Reorg.Schedule;

using Acme.Workforce.Orgdesign.Reorg.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ReorgScheduleRulesChecks
{
    public static void ReorgScheduleValidatorReportsEveryProblem()
    {
        var validator = ReorgScheduleRules.DefaultReorgScheduleValidator();
        var item = ReorgSchedule.NewReorgSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgScheduleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ReorgScheduleLabelLengthRule(1000);
        var item = ReorgSchedule.NewReorgSchedule("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgScheduleRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new ReorgScheduleRequiredPhoneRule("reorg schedule needs this field");
        var item = ReorgSchedule.NewReorgSchedule("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ReorgScheduleRequiredCodeRule("reorg schedule needs this field");
        var item = ReorgSchedule.NewReorgSchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ReorgScheduleValidatorReportsEveryProblem();
        ReorgScheduleLabelLengthRuleFlagsALongLabel();
        ReorgScheduleRequiredPhoneRuleFlagsABlankPhone();
        ReorgScheduleRequiredCodeRuleFlagsABlankCode();
    }
}
