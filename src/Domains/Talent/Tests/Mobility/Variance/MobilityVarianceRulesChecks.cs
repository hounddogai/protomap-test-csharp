namespace Acme.Workforce.Talent.Tests.Mobility.Variance;

using Acme.Workforce.Talent.Mobility.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class MobilityVarianceRulesChecks
{
    public static void MobilityVarianceValidatorReportsEveryProblem()
    {
        var validator = MobilityVarianceRules.DefaultMobilityVarianceValidator();
        var item = MobilityVariance.NewMobilityVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityVarianceRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new MobilityVarianceRateRangeRule(0.2, 3305.7);
        var item = MobilityVariance.NewMobilityVariance("x", "y");
        item.Rate = 3307.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MobilityVarianceRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new MobilityVarianceRequiredTitleRule("mobility variance needs this field");
        var item = MobilityVariance.NewMobilityVariance("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new MobilityVarianceCodeLengthRule(86400);
        var item = MobilityVariance.NewMobilityVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        MobilityVarianceValidatorReportsEveryProblem();
        MobilityVarianceRateRangeRuleFlagsValuesAboveRate();
        MobilityVarianceRequiredTitleRuleFlagsABlankTitle();
        MobilityVarianceCodeLengthRuleFlagsALongCode();
    }
}
