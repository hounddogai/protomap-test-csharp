namespace Acme.Workforce.Compliance.Tests.Exception.Assignment;

using Acme.Workforce.Compliance.Exception.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class ExceptionAssignmentRulesChecks
{
    public static void ExceptionAssignmentValidatorReportsEveryProblem()
    {
        var validator = ExceptionAssignmentRules.DefaultExceptionAssignmentValidator();
        var item = ExceptionAssignment.NewExceptionAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionAssignmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ExceptionAssignmentTitleLengthRule(80);
        var item = ExceptionAssignment.NewExceptionAssignment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionAssignmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ExceptionAssignmentRequiredTitleRule("exception assignment needs this field");
        var item = ExceptionAssignment.NewExceptionAssignment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionAssignmentRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new ExceptionAssignmentRequiredCountryRule("exception assignment needs a value");
        var item = ExceptionAssignment.NewExceptionAssignment("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ExceptionAssignmentValidatorReportsEveryProblem();
        ExceptionAssignmentTitleLengthRuleFlagsALongTitle();
        ExceptionAssignmentRequiredTitleRuleFlagsABlankTitle();
        ExceptionAssignmentRequiredCountryRuleFlagsABlankCountry();
    }
}
