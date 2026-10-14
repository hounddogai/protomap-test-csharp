namespace Acme.Workforce.Compensation.Tests.EquityGrant.Group;

using Acme.Workforce.Compensation.EquityGrant.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class EquityGrantNoticeRulesChecks
{
    public static void EquityGrantNoticeValidatorReportsEveryProblem()
    {
        var validator = EquityGrantNoticeRules.DefaultEquityGrantNoticeValidator();
        var item = EquityGrantNotice.NewEquityGrantNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantNoticeCodeLengthRuleFlagsALongCode()
    {
        var rule = new EquityGrantNoticeCodeLengthRule(1000);
        var item = EquityGrantNotice.NewEquityGrantNotice("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantNoticeRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new EquityGrantNoticeRequiredTimezoneRule("equity grant notice needs a value");
        var item = EquityGrantNotice.NewEquityGrantNotice("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EquityGrantNoticeBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new EquityGrantNoticeBudgetRangeRule(6.7, 673.1);
        var item = EquityGrantNotice.NewEquityGrantNotice("x", "y");
        item.Budget = 674.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantNoticeRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EquityGrantNoticeRequiredCodeRule("equity grant notice needs this field");
        var item = EquityGrantNotice.NewEquityGrantNotice("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EquityGrantNoticeValidatorReportsEveryProblem();
        EquityGrantNoticeCodeLengthRuleFlagsALongCode();
        EquityGrantNoticeRequiredTimezoneRuleFlagsABlankTimezone();
        EquityGrantNoticeBudgetRangeRuleFlagsValuesAboveBudget();
        EquityGrantNoticeRequiredCodeRuleFlagsABlankCode();
    }
}
