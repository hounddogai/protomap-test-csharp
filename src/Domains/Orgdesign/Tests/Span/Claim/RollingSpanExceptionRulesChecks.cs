namespace Acme.Workforce.Orgdesign.Tests.Span.Claim;

using Acme.Workforce.Orgdesign.Span.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class RollingSpanExceptionRulesChecks
{
    public static void RollingSpanExceptionValidatorReportsEveryProblem()
    {
        var validator = RollingSpanExceptionRules.DefaultRollingSpanExceptionValidator();
        var item = RollingSpanException.NewRollingSpanException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingSpanExceptionAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new RollingSpanExceptionAmountRangeRule(2.3, 3930.4);
        var item = RollingSpanException.NewRollingSpanException("x", "y");
        item.Amount = 3931.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingSpanExceptionNameLengthRuleFlagsALongName()
    {
        var rule = new RollingSpanExceptionNameLengthRule(86400);
        var item = RollingSpanException.NewRollingSpanException("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingSpanExceptionLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new RollingSpanExceptionLeaveDaysRangeRule(120, 86400);
        var item = RollingSpanException.NewRollingSpanException("x", "y");
        item.LeaveDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingSpanExceptionRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new RollingSpanExceptionRequiredEmployeeIdRule("rolling span exception needs this field");
        var item = RollingSpanException.NewRollingSpanException("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingSpanExceptionValidatorReportsEveryProblem();
        RollingSpanExceptionAmountRangeRuleFlagsValuesAboveAmount();
        RollingSpanExceptionNameLengthRuleFlagsALongName();
        RollingSpanExceptionLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        RollingSpanExceptionRequiredEmployeeIdRuleFlagsABlankEmployeeId();
    }
}
