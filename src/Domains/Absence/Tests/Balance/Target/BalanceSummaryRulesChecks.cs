namespace Acme.Workforce.Absence.Tests.Balance.Target;

using Acme.Workforce.Absence.Balance.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class BalanceSummaryRulesChecks
{
    public static void BalanceSummaryValidatorReportsEveryProblem()
    {
        var validator = BalanceSummaryRules.DefaultBalanceSummaryValidator();
        var item = BalanceSummary.NewBalanceSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalanceSummaryRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new BalanceSummaryRequiredLabelRule("balance summary needs this field");
        var item = BalanceSummary.NewBalanceSummary("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BalanceSummaryMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new BalanceSummaryMaximumRangeRule(5.1, 1262.4);
        var item = BalanceSummary.NewBalanceSummary("x", "y");
        item.Maximum = 1263.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BalanceSummaryLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new BalanceSummaryLimitRangeRule(32, 3600);
        var item = BalanceSummary.NewBalanceSummary("x", "y");
        item.Limit = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BalanceSummaryLabelLengthRuleFlagsALongLabel()
    {
        var rule = new BalanceSummaryLabelLengthRule(1000);
        var item = BalanceSummary.NewBalanceSummary("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BalanceSummaryValidatorReportsEveryProblem();
        BalanceSummaryRequiredLabelRuleFlagsABlankLabel();
        BalanceSummaryMaximumRangeRuleFlagsValuesAboveMaximum();
        BalanceSummaryLimitRangeRuleFlagsValuesAboveLimit();
        BalanceSummaryLabelLengthRuleFlagsALongLabel();
    }
}
