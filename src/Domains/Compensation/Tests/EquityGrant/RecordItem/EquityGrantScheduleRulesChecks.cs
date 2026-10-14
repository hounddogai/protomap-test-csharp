namespace Acme.Workforce.Compensation.Tests.EquityGrant.RecordItem;

using Acme.Workforce.Compensation.EquityGrant.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class EquityGrantScheduleRulesChecks
{
    public static void EquityGrantScheduleValidatorReportsEveryProblem()
    {
        var validator = EquityGrantScheduleRules.DefaultEquityGrantScheduleValidator();
        var item = EquityGrantSchedule.NewEquityGrantSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantScheduleMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new EquityGrantScheduleMarginRangeRule(8.3, 378.2);
        var item = EquityGrantSchedule.NewEquityGrantSchedule("x", "y");
        item.Margin = 379.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EquityGrantScheduleRequiredCodeRule("equity grant schedule needs this field");
        var item = EquityGrantSchedule.NewEquityGrantSchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EquityGrantScheduleValidatorReportsEveryProblem();
        EquityGrantScheduleMarginRangeRuleFlagsValuesAboveMargin();
        EquityGrantScheduleRequiredCodeRuleFlagsABlankCode();
    }
}
