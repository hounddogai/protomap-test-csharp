namespace Acme.Workforce.Talent.Tests.CareerPath.Exception;

using Acme.Workforce.Talent.CareerPath.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class CareerPathExceptionRulesChecks
{
    public static void CareerPathExceptionValidatorReportsEveryProblem()
    {
        var validator = CareerPathExceptionRules.DefaultCareerPathExceptionValidator();
        var item = CareerPathException.NewCareerPathException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CareerPathExceptionCodeLengthRuleFlagsALongCode()
    {
        var rule = new CareerPathExceptionCodeLengthRule(32);
        var item = CareerPathException.NewCareerPathException("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CareerPathExceptionReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new CareerPathExceptionReviewerLengthRule(1000);
        var item = CareerPathException.NewCareerPathException("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CareerPathExceptionStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new CareerPathExceptionStartsAtRangeRule(80, 1000);
        var item = CareerPathException.NewCareerPathException("x", "y");
        item.StartsAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CareerPathExceptionDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new CareerPathExceptionDurationMinutesRangeRule(64, 120);
        var item = CareerPathException.NewCareerPathException("x", "y");
        item.DurationMinutes = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CareerPathExceptionValidatorReportsEveryProblem();
        CareerPathExceptionCodeLengthRuleFlagsALongCode();
        CareerPathExceptionReviewerLengthRuleFlagsALongReviewer();
        CareerPathExceptionStartsAtRangeRuleFlagsValuesAboveStartsAt();
        CareerPathExceptionDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
    }
}
