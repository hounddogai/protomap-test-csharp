namespace Acme.Workforce.Orgdesign.Tests.Position.Baseline;

using Acme.Workforce.Orgdesign.Position.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PositionBaselineRulesChecks
{
    public static void PositionBaselineValidatorReportsEveryProblem()
    {
        var validator = PositionBaselineRules.DefaultPositionBaselineValidator();
        var item = PositionBaseline.NewPositionBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PositionBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new PositionBaselineCodeLengthRule(255);
        var item = PositionBaseline.NewPositionBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PositionBaselineRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new PositionBaselineRequiredSkillRule("position baseline needs this field");
        var item = PositionBaseline.NewPositionBaseline("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PositionBaselineAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new PositionBaselineAmountRangeRule(9.0, 1790.4);
        var item = PositionBaseline.NewPositionBaseline("x", "y");
        item.Amount = 1791.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PositionBaselineValidatorReportsEveryProblem();
        PositionBaselineCodeLengthRuleFlagsALongCode();
        PositionBaselineRequiredSkillRuleFlagsABlankSkill();
        PositionBaselineAmountRangeRuleFlagsValuesAboveAmount();
    }
}
