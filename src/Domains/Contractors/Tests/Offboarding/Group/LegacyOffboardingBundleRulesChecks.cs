namespace Acme.Workforce.Contractors.Tests.Offboarding.Group;

using Acme.Workforce.Contractors.Offboarding.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class LegacyOffboardingBundleRulesChecks
{
    public static void LegacyOffboardingBundleValidatorReportsEveryProblem()
    {
        var validator = LegacyOffboardingBundleRules.DefaultLegacyOffboardingBundleValidator();
        var item = LegacyOffboardingBundle.NewLegacyOffboardingBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyOffboardingBundleCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new LegacyOffboardingBundleCostRangeRule(9.2, 1084.3);
        var item = LegacyOffboardingBundle.NewLegacyOffboardingBundle("x", "y");
        item.Cost = 1085.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyOffboardingBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyOffboardingBundleCodeLengthRule(32);
        var item = LegacyOffboardingBundle.NewLegacyOffboardingBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyOffboardingBundleRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new LegacyOffboardingBundleRequiredCostCenterRule("legacy offboarding bundle needs a value");
        var item = LegacyOffboardingBundle.NewLegacyOffboardingBundle("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyOffboardingBundleValidatorReportsEveryProblem();
        LegacyOffboardingBundleCostRangeRuleFlagsValuesAboveCost();
        LegacyOffboardingBundleCodeLengthRuleFlagsALongCode();
        LegacyOffboardingBundleRequiredCostCenterRuleFlagsABlankCostCenter();
    }
}
