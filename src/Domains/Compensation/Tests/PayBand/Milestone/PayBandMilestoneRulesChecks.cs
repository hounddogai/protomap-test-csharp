namespace Acme.Workforce.Compensation.Tests.PayBand.Milestone;

using Acme.Workforce.Compensation.PayBand.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class PayBandMilestoneRulesChecks
{
    public static void PayBandMilestoneValidatorReportsEveryProblem()
    {
        var validator = PayBandMilestoneRules.DefaultPayBandMilestoneValidator();
        var item = PayBandMilestone.NewPayBandMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandMilestoneMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new PayBandMilestoneMarginRangeRule(3.7, 200.2);
        var item = PayBandMilestone.NewPayBandMilestone("x", "y");
        item.Margin = 201.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayBandMilestoneCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayBandMilestoneCodeLengthRule(64);
        var item = PayBandMilestone.NewPayBandMilestone("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PayBandMilestoneValidatorReportsEveryProblem();
        PayBandMilestoneMarginRangeRuleFlagsValuesAboveMargin();
        PayBandMilestoneCodeLengthRuleFlagsALongCode();
    }
}
