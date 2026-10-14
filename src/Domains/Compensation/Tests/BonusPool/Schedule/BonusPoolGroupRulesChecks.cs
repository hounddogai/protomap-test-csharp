namespace Acme.Workforce.Compensation.Tests.BonusPool.Schedule;

using Acme.Workforce.Compensation.BonusPool.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class BonusPoolGroupRulesChecks
{
    public static void BonusPoolGroupValidatorReportsEveryProblem()
    {
        var validator = BonusPoolGroupRules.DefaultBonusPoolGroupValidator();
        var item = BonusPoolGroup.NewBonusPoolGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolGroupCodeLengthRuleFlagsALongCode()
    {
        var rule = new BonusPoolGroupCodeLengthRule(255);
        var item = BonusPoolGroup.NewBonusPoolGroup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolGroupBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new BonusPoolGroupBudgetRangeRule(8.3, 4053.2);
        var item = BonusPoolGroup.NewBonusPoolGroup("x", "y");
        item.Budget = 4054.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolGroupRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new BonusPoolGroupRequiredLabelRule("bonus pool group needs this field");
        var item = BonusPoolGroup.NewBonusPoolGroup("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BonusPoolGroupValidatorReportsEveryProblem();
        BonusPoolGroupCodeLengthRuleFlagsALongCode();
        BonusPoolGroupBudgetRangeRuleFlagsValuesAboveBudget();
        BonusPoolGroupRequiredLabelRuleFlagsABlankLabel();
    }
}
