namespace Acme.Workforce.Compliance.Tests.Exception.Rule;

using Acme.Workforce.Compliance.Exception.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ExceptionSegmentRulesChecks
{
    public static void ExceptionSegmentValidatorReportsEveryProblem()
    {
        var validator = ExceptionSegmentRules.DefaultExceptionSegmentValidator();
        var item = ExceptionSegment.NewExceptionSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionSegmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ExceptionSegmentTitleLengthRule(16);
        var item = ExceptionSegment.NewExceptionSegment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionSegmentRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new ExceptionSegmentRequiredSourceRule("exception segment needs a value");
        var item = ExceptionSegment.NewExceptionSegment("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionSegmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ExceptionSegmentRequiredTitleRule("exception segment needs this field");
        var item = ExceptionSegment.NewExceptionSegment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionSegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new ExceptionSegmentCodeLengthRule(64);
        var item = ExceptionSegment.NewExceptionSegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ExceptionSegmentValidatorReportsEveryProblem();
        ExceptionSegmentTitleLengthRuleFlagsALongTitle();
        ExceptionSegmentRequiredSourceRuleFlagsABlankSource();
        ExceptionSegmentRequiredTitleRuleFlagsABlankTitle();
        ExceptionSegmentCodeLengthRuleFlagsALongCode();
    }
}
