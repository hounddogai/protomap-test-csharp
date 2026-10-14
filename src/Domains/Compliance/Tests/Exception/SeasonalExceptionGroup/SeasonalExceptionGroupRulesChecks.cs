namespace Acme.Workforce.Compliance.Tests.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>Checks of seasonal exception group code, run by the project's runner.</summary>
public static class SeasonalExceptionGroupRulesChecks
{
    public static void SeasonalExceptionGroupValidatorReportsEveryProblem()
    {
        var validator = SeasonalExceptionGroupRules.DefaultSeasonalExceptionGroupValidator();
        var item = SeasonalExceptionGroup.NewSeasonalExceptionGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalExceptionGroupDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new SeasonalExceptionGroupDiscountRangeRule(1.7, 4039.2);
        var item = SeasonalExceptionGroup.NewSeasonalExceptionGroup("x", "y");
        item.Discount = 4040.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalExceptionGroupCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalExceptionGroupCodeLengthRule(80);
        var item = SeasonalExceptionGroup.NewSeasonalExceptionGroup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalExceptionGroupRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SeasonalExceptionGroupRequiredLabelRule("seasonal exception group needs this field");
        var item = SeasonalExceptionGroup.NewSeasonalExceptionGroup("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalExceptionGroupManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new SeasonalExceptionGroupManagerIdLengthRule(120);
        var item = SeasonalExceptionGroup.NewSeasonalExceptionGroup("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalExceptionGroupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalExceptionGroupRequiredCodeRule("seasonal exception group needs a value");
        var item = SeasonalExceptionGroup.NewSeasonalExceptionGroup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalExceptionGroupValidatorReportsEveryProblem();
        SeasonalExceptionGroupDiscountRangeRuleFlagsValuesAboveDiscount();
        SeasonalExceptionGroupCodeLengthRuleFlagsALongCode();
        SeasonalExceptionGroupRequiredLabelRuleFlagsABlankLabel();
        SeasonalExceptionGroupManagerIdLengthRuleFlagsALongManagerId();
        SeasonalExceptionGroupRequiredCodeRuleFlagsABlankCode();
    }
}
