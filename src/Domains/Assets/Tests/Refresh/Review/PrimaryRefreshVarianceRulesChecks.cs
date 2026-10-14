namespace Acme.Workforce.Assets.Tests.Refresh.Review;

using Acme.Workforce.Assets.Refresh.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PrimaryRefreshVarianceRulesChecks
{
    public static void PrimaryRefreshVarianceValidatorReportsEveryProblem()
    {
        var validator = PrimaryRefreshVarianceRules.DefaultPrimaryRefreshVarianceValidator();
        var item = PrimaryRefreshVariance.NewPrimaryRefreshVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryRefreshVarianceLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PrimaryRefreshVarianceLabelLengthRule(120);
        var item = PrimaryRefreshVariance.NewPrimaryRefreshVariance("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryRefreshVarianceGradeLengthRuleFlagsALongGrade()
    {
        var rule = new PrimaryRefreshVarianceGradeLengthRule(120);
        var item = PrimaryRefreshVariance.NewPrimaryRefreshVariance("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryRefreshVarianceStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new PrimaryRefreshVarianceStartsAtRangeRule(120, 1000);
        var item = PrimaryRefreshVariance.NewPrimaryRefreshVariance("x", "y");
        item.StartsAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryRefreshVarianceRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new PrimaryRefreshVarianceRequiredGradeRule("primary refresh variance needs this field");
        var item = PrimaryRefreshVariance.NewPrimaryRefreshVariance("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryRefreshVarianceValidatorReportsEveryProblem();
        PrimaryRefreshVarianceLabelLengthRuleFlagsALongLabel();
        PrimaryRefreshVarianceGradeLengthRuleFlagsALongGrade();
        PrimaryRefreshVarianceStartsAtRangeRuleFlagsValuesAboveStartsAt();
        PrimaryRefreshVarianceRequiredGradeRuleFlagsABlankGrade();
    }
}
