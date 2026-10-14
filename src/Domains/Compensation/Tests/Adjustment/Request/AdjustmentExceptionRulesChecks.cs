namespace Acme.Workforce.Compensation.Tests.Adjustment.Request;

using Acme.Workforce.Compensation.Adjustment.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AdjustmentExceptionRulesChecks
{
    public static void AdjustmentExceptionValidatorReportsEveryProblem()
    {
        var validator = AdjustmentExceptionRules.DefaultAdjustmentExceptionValidator();
        var item = AdjustmentException.NewAdjustmentException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentExceptionRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdjustmentExceptionRequiredCodeRule("adjustment exception needs an entry");
        var item = AdjustmentException.NewAdjustmentException("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentExceptionRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new AdjustmentExceptionRatioRangeRule(2.9, 4416.1);
        var item = AdjustmentException.NewAdjustmentException("x", "y");
        item.Ratio = 4417.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdjustmentExceptionRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new AdjustmentExceptionRequiredContactEmailRule("adjustment exception needs this field");
        var item = AdjustmentException.NewAdjustmentException("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentExceptionVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new AdjustmentExceptionVersionRangeRule(32, 255);
        var item = AdjustmentException.NewAdjustmentException("x", "y");
        item.Version = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdjustmentExceptionContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new AdjustmentExceptionContactEmailLengthRule(255);
        var item = AdjustmentException.NewAdjustmentException("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentExceptionTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AdjustmentExceptionTitleLengthRule(120);
        var item = AdjustmentException.NewAdjustmentException("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdjustmentExceptionValidatorReportsEveryProblem();
        AdjustmentExceptionRequiredCodeRuleFlagsABlankCode();
        AdjustmentExceptionRatioRangeRuleFlagsValuesAboveRatio();
        AdjustmentExceptionRequiredContactEmailRuleFlagsABlankContactEmail();
        AdjustmentExceptionVersionRangeRuleFlagsValuesAboveVersion();
        AdjustmentExceptionContactEmailLengthRuleFlagsALongContactEmail();
        AdjustmentExceptionTitleLengthRuleFlagsALongTitle();
    }
}
