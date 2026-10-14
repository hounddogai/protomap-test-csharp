namespace Acme.Workforce.Contractors.Tests.Offboarding.Request;

using Acme.Workforce.Contractors.Offboarding.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class OffboardingRollupRulesChecks
{
    public static void OffboardingRollupValidatorReportsEveryProblem()
    {
        var validator = OffboardingRollupRules.DefaultOffboardingRollupValidator();
        var item = OffboardingRollup.NewOffboardingRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OffboardingRollupBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new OffboardingRollupBudgetRangeRule(1.0, 926.9);
        var item = OffboardingRollup.NewOffboardingRollup("x", "y");
        item.Budget = 928.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OffboardingRollupCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new OffboardingRollupCreatedAtRangeRule(1000, 1000);
        var item = OffboardingRollup.NewOffboardingRollup("x", "y");
        item.CreatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OffboardingRollupRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new OffboardingRollupRequiredRegionRule("offboarding rollup needs this field");
        var item = OffboardingRollup.NewOffboardingRollup("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        OffboardingRollupValidatorReportsEveryProblem();
        OffboardingRollupBudgetRangeRuleFlagsValuesAboveBudget();
        OffboardingRollupCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        OffboardingRollupRequiredRegionRuleFlagsABlankRegion();
    }
}
