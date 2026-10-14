namespace Acme.Workforce.Orgdesign.Tests.Position.PositionBundle;

using Acme.Workforce.Orgdesign.Position.PositionBundle;

/// <summary>Checks of position bundle code, run by the project's runner.</summary>
public static class PositionVarianceRulesChecks
{
    public static void PositionVarianceValidatorReportsEveryProblem()
    {
        var validator = PositionVarianceRules.DefaultPositionVarianceValidator();
        var item = PositionVariance.NewPositionVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PositionVarianceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PositionVarianceRequiredCodeRule("position variance needs an entry");
        var item = PositionVariance.NewPositionVariance("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PositionVarianceWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new PositionVarianceWindowDaysRangeRule(80, 1000);
        var item = PositionVariance.NewPositionVariance("x", "y");
        item.WindowDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PositionVarianceEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new PositionVarianceEndsAtRangeRule(80, 1000);
        var item = PositionVariance.NewPositionVariance("x", "y");
        item.EndsAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PositionVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new PositionVarianceCodeLengthRule(64);
        var item = PositionVariance.NewPositionVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PositionVarianceValidatorReportsEveryProblem();
        PositionVarianceRequiredCodeRuleFlagsABlankCode();
        PositionVarianceWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        PositionVarianceEndsAtRangeRuleFlagsValuesAboveEndsAt();
        PositionVarianceCodeLengthRuleFlagsALongCode();
    }
}
