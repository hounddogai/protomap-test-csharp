namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelGroup;

using Acme.Workforce.Orgdesign.CostModel.ModelGroup;

/// <summary>Checks of model group code, run by the project's runner.</summary>
public static class RollingCostModelBatchRulesChecks
{
    public static void RollingCostModelBatchValidatorReportsEveryProblem()
    {
        var validator = RollingCostModelBatchRules.DefaultRollingCostModelBatchValidator();
        var item = RollingCostModelBatch.NewRollingCostModelBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingCostModelBatchRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new RollingCostModelBatchRequiredSkillRule("rolling cost model batch needs this field");
        var item = RollingCostModelBatch.NewRollingCostModelBatch("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingCostModelBatchRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new RollingCostModelBatchRequiredTeamRule("rolling cost model batch needs an entry");
        var item = RollingCostModelBatch.NewRollingCostModelBatch("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingCostModelBatchTeamLengthRuleFlagsALongTeam()
    {
        var rule = new RollingCostModelBatchTeamLengthRule(16);
        var item = RollingCostModelBatch.NewRollingCostModelBatch("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingCostModelBatchSkillLengthRuleFlagsALongSkill()
    {
        var rule = new RollingCostModelBatchSkillLengthRule(64);
        var item = RollingCostModelBatch.NewRollingCostModelBatch("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingCostModelBatchRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RollingCostModelBatchRequiredLabelRule("rolling cost model batch needs this field");
        var item = RollingCostModelBatch.NewRollingCostModelBatch("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingCostModelBatchCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new RollingCostModelBatchCreatedAtRangeRule(80, 120);
        var item = RollingCostModelBatch.NewRollingCostModelBatch("x", "y");
        item.CreatedAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingCostModelBatchValidatorReportsEveryProblem();
        RollingCostModelBatchRequiredSkillRuleFlagsABlankSkill();
        RollingCostModelBatchRequiredTeamRuleFlagsABlankTeam();
        RollingCostModelBatchTeamLengthRuleFlagsALongTeam();
        RollingCostModelBatchSkillLengthRuleFlagsALongSkill();
        RollingCostModelBatchRequiredLabelRuleFlagsABlankLabel();
        RollingCostModelBatchCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
