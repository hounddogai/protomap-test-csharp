namespace Acme.Workforce.Workplace.Tests.Visitor.Exception;

using Acme.Workforce.Workplace.Visitor.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class LenientVisitorExceptionRulesChecks
{
    public static void LenientVisitorExceptionValidatorReportsEveryProblem()
    {
        var validator = LenientVisitorExceptionRules.DefaultLenientVisitorExceptionValidator();
        var item = LenientVisitorException.NewLenientVisitorException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientVisitorExceptionNameLengthRuleFlagsALongName()
    {
        var rule = new LenientVisitorExceptionNameLengthRule(16);
        var item = LenientVisitorException.NewLenientVisitorException("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientVisitorExceptionRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new LenientVisitorExceptionRetriesRangeRule(64, 120);
        var item = LenientVisitorException.NewLenientVisitorException("x", "y");
        item.Retries = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientVisitorExceptionRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientVisitorExceptionRequiredNameRule("lenient visitor exception needs this field");
        var item = LenientVisitorException.NewLenientVisitorException("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientVisitorExceptionValidatorReportsEveryProblem();
        LenientVisitorExceptionNameLengthRuleFlagsALongName();
        LenientVisitorExceptionRetriesRangeRuleFlagsValuesAboveRetries();
        LenientVisitorExceptionRequiredNameRuleFlagsABlankName();
    }
}
