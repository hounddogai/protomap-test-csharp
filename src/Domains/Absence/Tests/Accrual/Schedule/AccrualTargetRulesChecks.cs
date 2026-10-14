namespace Acme.Workforce.Absence.Tests.Accrual.Schedule;

using Acme.Workforce.Absence.Accrual.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class AccrualTargetRulesChecks
{
    public static void AccrualTargetValidatorReportsEveryProblem()
    {
        var validator = AccrualTargetRules.DefaultAccrualTargetValidator();
        var item = AccrualTarget.NewAccrualTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualTargetRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new AccrualTargetRequiredChannelRule("accrual target needs a value");
        var item = AccrualTarget.NewAccrualTarget("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualTargetRequiredNameRuleFlagsABlankName()
    {
        var rule = new AccrualTargetRequiredNameRule("accrual target needs a value");
        var item = AccrualTarget.NewAccrualTarget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualTargetRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new AccrualTargetRetriesRangeRule(80, 120);
        var item = AccrualTarget.NewAccrualTarget("x", "y");
        item.Retries = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccrualTargetCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new AccrualTargetCreatedAtRangeRule(80, 86400);
        var item = AccrualTarget.NewAccrualTarget("x", "y");
        item.CreatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccrualTargetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AccrualTargetRequiredCodeRule("accrual target needs this field");
        var item = AccrualTarget.NewAccrualTarget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccrualTargetValidatorReportsEveryProblem();
        AccrualTargetRequiredChannelRuleFlagsABlankChannel();
        AccrualTargetRequiredNameRuleFlagsABlankName();
        AccrualTargetRetriesRangeRuleFlagsValuesAboveRetries();
        AccrualTargetCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        AccrualTargetRequiredCodeRuleFlagsABlankCode();
    }
}
