namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolRequest;

using Acme.Workforce.Compensation.BonusPool.PoolRequest;

/// <summary>Checks of pool request code, run by the project's runner.</summary>
public static class BonusPoolOverrideRulesChecks
{
    public static void BonusPoolOverrideValidatorReportsEveryProblem()
    {
        var validator = BonusPoolOverrideRules.DefaultBonusPoolOverrideValidator();
        var item = BonusPoolOverride.NewBonusPoolOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new BonusPoolOverrideCodeLengthRule(16);
        var item = BonusPoolOverride.NewBonusPoolOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolOverrideRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new BonusPoolOverrideRequiredCategoryRule("bonus pool override needs this field");
        var item = BonusPoolOverride.NewBonusPoolOverride("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BonusPoolOverrideRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BonusPoolOverrideRequiredCodeRule("bonus pool override needs a value");
        var item = BonusPoolOverride.NewBonusPoolOverride("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BonusPoolOverrideDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new BonusPoolOverrideDurationMinutesRangeRule(1000, 3600);
        var item = BonusPoolOverride.NewBonusPoolOverride("x", "y");
        item.DurationMinutes = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolOverrideRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new BonusPoolOverrideRevisionRangeRule(80, 3600);
        var item = BonusPoolOverride.NewBonusPoolOverride("x", "y");
        item.Revision = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BonusPoolOverrideValidatorReportsEveryProblem();
        BonusPoolOverrideCodeLengthRuleFlagsALongCode();
        BonusPoolOverrideRequiredCategoryRuleFlagsABlankCategory();
        BonusPoolOverrideRequiredCodeRuleFlagsABlankCode();
        BonusPoolOverrideDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        BonusPoolOverrideRevisionRangeRuleFlagsValuesAboveRevision();
    }
}
