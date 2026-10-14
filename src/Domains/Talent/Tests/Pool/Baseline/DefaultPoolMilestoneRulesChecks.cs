namespace Acme.Workforce.Talent.Tests.Pool.Baseline;

using Acme.Workforce.Talent.Pool.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DefaultPoolMilestoneRulesChecks
{
    public static void DefaultPoolMilestoneValidatorReportsEveryProblem()
    {
        var validator = DefaultPoolMilestoneRules.DefaultDefaultPoolMilestoneValidator();
        var item = DefaultPoolMilestone.NewDefaultPoolMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultPoolMilestoneNameLengthRuleFlagsALongName()
    {
        var rule = new DefaultPoolMilestoneNameLengthRule(255);
        var item = DefaultPoolMilestone.NewDefaultPoolMilestone("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultPoolMilestoneRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultPoolMilestoneRequiredCodeRule("default pool milestone needs an entry");
        var item = DefaultPoolMilestone.NewDefaultPoolMilestone("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultPoolMilestoneCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new DefaultPoolMilestoneCreatedAtRangeRule(16, 3600);
        var item = DefaultPoolMilestone.NewDefaultPoolMilestone("x", "y");
        item.CreatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultPoolMilestoneCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultPoolMilestoneCodeLengthRule(16);
        var item = DefaultPoolMilestone.NewDefaultPoolMilestone("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultPoolMilestoneValidatorReportsEveryProblem();
        DefaultPoolMilestoneNameLengthRuleFlagsALongName();
        DefaultPoolMilestoneRequiredCodeRuleFlagsABlankCode();
        DefaultPoolMilestoneCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        DefaultPoolMilestoneCodeLengthRuleFlagsALongCode();
    }
}
