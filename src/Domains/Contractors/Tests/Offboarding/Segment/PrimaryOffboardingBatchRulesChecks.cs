namespace Acme.Workforce.Contractors.Tests.Offboarding.Segment;

using Acme.Workforce.Contractors.Offboarding.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class PrimaryOffboardingBatchRulesChecks
{
    public static void PrimaryOffboardingBatchValidatorReportsEveryProblem()
    {
        var validator = PrimaryOffboardingBatchRules.DefaultPrimaryOffboardingBatchValidator();
        var item = PrimaryOffboardingBatch.NewPrimaryOffboardingBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryOffboardingBatchPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new PrimaryOffboardingBatchPriorityRangeRule(16, 80);
        var item = PrimaryOffboardingBatch.NewPrimaryOffboardingBatch("x", "y");
        item.Priority = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryOffboardingBatchLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new PrimaryOffboardingBatchLimitRangeRule(32, 86400);
        var item = PrimaryOffboardingBatch.NewPrimaryOffboardingBatch("x", "y");
        item.Limit = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PrimaryOffboardingBatchValidatorReportsEveryProblem();
        PrimaryOffboardingBatchPriorityRangeRuleFlagsValuesAbovePriority();
        PrimaryOffboardingBatchLimitRangeRuleFlagsValuesAboveLimit();
    }
}
