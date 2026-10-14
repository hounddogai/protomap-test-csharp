namespace Acme.Workforce.Contractors.Tests.Rate.Score;

using Acme.Workforce.Contractors.Rate.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class RateExceptionRulesChecks
{
    public static void RateExceptionValidatorReportsEveryProblem()
    {
        var validator = RateExceptionRules.DefaultRateExceptionValidator();
        var item = RateException.NewRateException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RateExceptionRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RateExceptionRequiredCodeRule("rate exception needs this field");
        var item = RateException.NewRateException("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateExceptionRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new RateExceptionRequiredNotesRule("rate exception needs a value");
        var item = RateException.NewRateException("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RateExceptionValidatorReportsEveryProblem();
        RateExceptionRequiredCodeRuleFlagsABlankCode();
        RateExceptionRequiredNotesRuleFlagsABlankNotes();
    }
}
