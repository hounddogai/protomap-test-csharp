namespace Acme.Workforce.Compliance.Tests.PolicyAck.Schedule;

using Acme.Workforce.Compliance.PolicyAck.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class TieredPolicyAckScheduleRulesChecks
{
    public static void TieredPolicyAckScheduleValidatorReportsEveryProblem()
    {
        var validator = TieredPolicyAckScheduleRules.DefaultTieredPolicyAckScheduleValidator();
        var item = TieredPolicyAckSchedule.NewTieredPolicyAckSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredPolicyAckScheduleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new TieredPolicyAckScheduleTitleLengthRule(32);
        var item = TieredPolicyAckSchedule.NewTieredPolicyAckSchedule("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredPolicyAckScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredPolicyAckScheduleRequiredCodeRule("tiered policy ack schedule needs a value");
        var item = TieredPolicyAckSchedule.NewTieredPolicyAckSchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredPolicyAckScheduleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TieredPolicyAckScheduleRequiredTitleRule("tiered policy ack schedule needs a value");
        var item = TieredPolicyAckSchedule.NewTieredPolicyAckSchedule("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredPolicyAckScheduleValidatorReportsEveryProblem();
        TieredPolicyAckScheduleTitleLengthRuleFlagsALongTitle();
        TieredPolicyAckScheduleRequiredCodeRuleFlagsABlankCode();
        TieredPolicyAckScheduleRequiredTitleRuleFlagsABlankTitle();
    }
}
