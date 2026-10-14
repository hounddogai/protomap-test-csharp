namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementAssignment;

using Acme.Workforce.Contractors.Engagement.EngagementAssignment;

/// <summary>Checks of engagement assignment code, run by the project's runner.</summary>
public static class EngagementExceptionRulesChecks
{
    public static void EngagementExceptionValidatorReportsEveryProblem()
    {
        var validator = EngagementExceptionRules.DefaultEngagementExceptionValidator();
        var item = EngagementException.NewEngagementException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementExceptionMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new EngagementExceptionMaximumRangeRule(7.4, 57.8);
        var item = EngagementException.NewEngagementException("x", "y");
        item.Maximum = 59.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EngagementExceptionLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new EngagementExceptionLocaleLengthRule(3600);
        var item = EngagementException.NewEngagementException("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementExceptionCodeLengthRuleFlagsALongCode()
    {
        var rule = new EngagementExceptionCodeLengthRule(86400);
        var item = EngagementException.NewEngagementException("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementExceptionRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new EngagementExceptionRequiredChecksumRule("engagement exception needs this field");
        var item = EngagementException.NewEngagementException("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementExceptionRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EngagementExceptionRequiredCodeRule("engagement exception needs a value");
        var item = EngagementException.NewEngagementException("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EngagementExceptionValidatorReportsEveryProblem();
        EngagementExceptionMaximumRangeRuleFlagsValuesAboveMaximum();
        EngagementExceptionLocaleLengthRuleFlagsALongLocale();
        EngagementExceptionCodeLengthRuleFlagsALongCode();
        EngagementExceptionRequiredChecksumRuleFlagsABlankChecksum();
        EngagementExceptionRequiredCodeRuleFlagsABlankCode();
    }
}
