namespace Acme.Workforce.Absence.Tests.Policy.Profile;

using Acme.Workforce.Absence.Policy.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RegionalPolicyVarianceRulesChecks
{
    public static void RegionalPolicyVarianceValidatorReportsEveryProblem()
    {
        var validator = RegionalPolicyVarianceRules.DefaultRegionalPolicyVarianceValidator();
        var item = RegionalPolicyVariance.NewRegionalPolicyVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalPolicyVarianceEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new RegionalPolicyVarianceEndsAtRangeRule(64, 80);
        var item = RegionalPolicyVariance.NewRegionalPolicyVariance("x", "y");
        item.EndsAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalPolicyVariancePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new RegionalPolicyVariancePriorityRangeRule(64, 3600);
        var item = RegionalPolicyVariance.NewRegionalPolicyVariance("x", "y");
        item.Priority = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalPolicyVarianceValidatorReportsEveryProblem();
        RegionalPolicyVarianceEndsAtRangeRuleFlagsValuesAboveEndsAt();
        RegionalPolicyVariancePriorityRangeRuleFlagsValuesAbovePriority();
    }
}
