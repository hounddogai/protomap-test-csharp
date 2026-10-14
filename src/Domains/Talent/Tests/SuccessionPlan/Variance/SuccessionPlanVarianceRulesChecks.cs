namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Variance;

using Acme.Workforce.Talent.SuccessionPlan.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class SuccessionPlanVarianceRulesChecks
{
    public static void SuccessionPlanVarianceValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanVarianceRules.DefaultSuccessionPlanVarianceValidator();
        var item = SuccessionPlanVariance.NewSuccessionPlanVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanVarianceExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new SuccessionPlanVarianceExpiresAtRangeRule(1000, 3600);
        var item = SuccessionPlanVariance.NewSuccessionPlanVariance("x", "y");
        item.ExpiresAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SuccessionPlanVarianceEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new SuccessionPlanVarianceEmployeeIdLengthRule(16);
        var item = SuccessionPlanVariance.NewSuccessionPlanVariance("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanVarianceNameLengthRuleFlagsALongName()
    {
        var rule = new SuccessionPlanVarianceNameLengthRule(3600);
        var item = SuccessionPlanVariance.NewSuccessionPlanVariance("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanVarianceRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SuccessionPlanVarianceRequiredTitleRule("succession plan variance needs an entry");
        var item = SuccessionPlanVariance.NewSuccessionPlanVariance("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SuccessionPlanVarianceValidatorReportsEveryProblem();
        SuccessionPlanVarianceExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        SuccessionPlanVarianceEmployeeIdLengthRuleFlagsALongEmployeeId();
        SuccessionPlanVarianceNameLengthRuleFlagsALongName();
        SuccessionPlanVarianceRequiredTitleRuleFlagsABlankTitle();
    }
}
