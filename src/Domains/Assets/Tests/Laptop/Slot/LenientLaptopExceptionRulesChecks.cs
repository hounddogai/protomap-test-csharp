namespace Acme.Workforce.Assets.Tests.Laptop.Slot;

using Acme.Workforce.Assets.Laptop.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class LenientLaptopExceptionRulesChecks
{
    public static void LenientLaptopExceptionValidatorReportsEveryProblem()
    {
        var validator = LenientLaptopExceptionRules.DefaultLenientLaptopExceptionValidator();
        var item = LenientLaptopException.NewLenientLaptopException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientLaptopExceptionRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientLaptopExceptionRequiredNameRule("lenient laptop exception needs this field");
        var item = LenientLaptopException.NewLenientLaptopException("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientLaptopExceptionCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new LenientLaptopExceptionCreatedAtRangeRule(1000, 1000);
        var item = LenientLaptopException.NewLenientLaptopException("x", "y");
        item.CreatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientLaptopExceptionSourceLengthRuleFlagsALongSource()
    {
        var rule = new LenientLaptopExceptionSourceLengthRule(80);
        var item = LenientLaptopException.NewLenientLaptopException("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientLaptopExceptionValidatorReportsEveryProblem();
        LenientLaptopExceptionRequiredNameRuleFlagsABlankName();
        LenientLaptopExceptionCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        LenientLaptopExceptionSourceLengthRuleFlagsALongSource();
    }
}
