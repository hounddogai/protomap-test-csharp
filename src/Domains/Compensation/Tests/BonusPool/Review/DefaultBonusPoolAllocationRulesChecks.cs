namespace Acme.Workforce.Compensation.Tests.BonusPool.Review;

using Acme.Workforce.Compensation.BonusPool.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class DefaultBonusPoolAllocationRulesChecks
{
    public static void DefaultBonusPoolAllocationValidatorReportsEveryProblem()
    {
        var validator = DefaultBonusPoolAllocationRules.DefaultDefaultBonusPoolAllocationValidator();
        var item = DefaultBonusPoolAllocation.NewDefaultBonusPoolAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultBonusPoolAllocationRequiredNameRuleFlagsABlankName()
    {
        var rule = new DefaultBonusPoolAllocationRequiredNameRule("default bonus pool allocation needs an entry");
        var item = DefaultBonusPoolAllocation.NewDefaultBonusPoolAllocation("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultBonusPoolAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultBonusPoolAllocationCodeLengthRule(3600);
        var item = DefaultBonusPoolAllocation.NewDefaultBonusPoolAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultBonusPoolAllocationRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new DefaultBonusPoolAllocationRequiredDescriptionRule("default bonus pool allocation needs an entry");
        var item = DefaultBonusPoolAllocation.NewDefaultBonusPoolAllocation("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultBonusPoolAllocationMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new DefaultBonusPoolAllocationMaximumRangeRule(0.2, 3072.8);
        var item = DefaultBonusPoolAllocation.NewDefaultBonusPoolAllocation("x", "y");
        item.Maximum = 3074.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultBonusPoolAllocationTeamLengthRuleFlagsALongTeam()
    {
        var rule = new DefaultBonusPoolAllocationTeamLengthRule(1000);
        var item = DefaultBonusPoolAllocation.NewDefaultBonusPoolAllocation("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultBonusPoolAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultBonusPoolAllocationRequiredCodeRule("default bonus pool allocation needs this field");
        var item = DefaultBonusPoolAllocation.NewDefaultBonusPoolAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultBonusPoolAllocationValidatorReportsEveryProblem();
        DefaultBonusPoolAllocationRequiredNameRuleFlagsABlankName();
        DefaultBonusPoolAllocationCodeLengthRuleFlagsALongCode();
        DefaultBonusPoolAllocationRequiredDescriptionRuleFlagsABlankDescription();
        DefaultBonusPoolAllocationMaximumRangeRuleFlagsValuesAboveMaximum();
        DefaultBonusPoolAllocationTeamLengthRuleFlagsALongTeam();
        DefaultBonusPoolAllocationRequiredCodeRuleFlagsABlankCode();
    }
}
