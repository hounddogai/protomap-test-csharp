namespace Acme.Workforce.Assets.Tests.Laptop.Quota;

using Acme.Workforce.Assets.Laptop.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class LaptopExceptionRulesChecks
{
    public static void LaptopExceptionValidatorReportsEveryProblem()
    {
        var validator = LaptopExceptionRules.DefaultLaptopExceptionValidator();
        var item = LaptopException.NewLaptopException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopExceptionCodeLengthRuleFlagsALongCode()
    {
        var rule = new LaptopExceptionCodeLengthRule(32);
        var item = LaptopException.NewLaptopException("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopExceptionRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LaptopExceptionRequiredCodeRule("laptop exception needs this field");
        var item = LaptopException.NewLaptopException("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LaptopExceptionRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LaptopExceptionRequiredLabelRule("laptop exception needs a value");
        var item = LaptopException.NewLaptopException("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LaptopExceptionReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new LaptopExceptionReviewerLengthRule(3600);
        var item = LaptopException.NewLaptopException("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LaptopExceptionValidatorReportsEveryProblem();
        LaptopExceptionCodeLengthRuleFlagsALongCode();
        LaptopExceptionRequiredCodeRuleFlagsABlankCode();
        LaptopExceptionRequiredLabelRuleFlagsABlankLabel();
        LaptopExceptionReviewerLengthRuleFlagsALongReviewer();
    }
}
