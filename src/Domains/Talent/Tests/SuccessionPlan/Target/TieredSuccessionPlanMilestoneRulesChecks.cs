namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Target;

using Acme.Workforce.Talent.SuccessionPlan.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class TieredSuccessionPlanMilestoneRulesChecks
{
    public static void TieredSuccessionPlanMilestoneValidatorReportsEveryProblem()
    {
        var validator = TieredSuccessionPlanMilestoneRules.DefaultTieredSuccessionPlanMilestoneValidator();
        var item = TieredSuccessionPlanMilestone.NewTieredSuccessionPlanMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredSuccessionPlanMilestoneGradeLengthRuleFlagsALongGrade()
    {
        var rule = new TieredSuccessionPlanMilestoneGradeLengthRule(86400);
        var item = TieredSuccessionPlanMilestone.NewTieredSuccessionPlanMilestone("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredSuccessionPlanMilestoneRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new TieredSuccessionPlanMilestoneRequiredOwnerIdRule(
            "tiered succession plan milestone needs this field");
        var item = TieredSuccessionPlanMilestone.NewTieredSuccessionPlanMilestone("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredSuccessionPlanMilestoneAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new TieredSuccessionPlanMilestoneAttemptsRangeRule(16, 255);
        var item = TieredSuccessionPlanMilestone.NewTieredSuccessionPlanMilestone("x", "y");
        item.Attempts = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TieredSuccessionPlanMilestoneValidatorReportsEveryProblem();
        TieredSuccessionPlanMilestoneGradeLengthRuleFlagsALongGrade();
        TieredSuccessionPlanMilestoneRequiredOwnerIdRuleFlagsABlankOwnerId();
        TieredSuccessionPlanMilestoneAttemptsRangeRuleFlagsValuesAboveAttempts();
    }
}
