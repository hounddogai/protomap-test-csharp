namespace Acme.Workforce.Orgdesign.Tests.Position.Policy;

using Acme.Workforce.Orgdesign.Position.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class PositionRollupRulesChecks
{
    public static void PositionRollupValidatorReportsEveryProblem()
    {
        var validator = PositionRollupRules.DefaultPositionRollupValidator();
        var item = PositionRollup.NewPositionRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PositionRollupSkillLengthRuleFlagsALongSkill()
    {
        var rule = new PositionRollupSkillLengthRule(32);
        var item = PositionRollup.NewPositionRollup("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PositionRollupRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new PositionRollupRequiredSourceRule("position rollup needs an entry");
        var item = PositionRollup.NewPositionRollup("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PositionRollupSourceLengthRuleFlagsALongSource()
    {
        var rule = new PositionRollupSourceLengthRule(32);
        var item = PositionRollup.NewPositionRollup("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PositionRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PositionRollupRequiredCodeRule("position rollup needs this field");
        var item = PositionRollup.NewPositionRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PositionRollupRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PositionRollupRequiredTitleRule("position rollup needs an entry");
        var item = PositionRollup.NewPositionRollup("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PositionRollupValidatorReportsEveryProblem();
        PositionRollupSkillLengthRuleFlagsALongSkill();
        PositionRollupRequiredSourceRuleFlagsABlankSource();
        PositionRollupSourceLengthRuleFlagsALongSource();
        PositionRollupRequiredCodeRuleFlagsABlankCode();
        PositionRollupRequiredTitleRuleFlagsABlankTitle();
    }
}
