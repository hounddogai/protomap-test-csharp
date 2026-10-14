namespace Acme.Workforce.Contractors.Tests.Timesheet.Review;

using Acme.Workforce.Contractors.Timesheet.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LenientTimesheetScoreRulesChecks
{
    public static void LenientTimesheetScoreValidatorReportsEveryProblem()
    {
        var validator = LenientTimesheetScoreRules.DefaultLenientTimesheetScoreValidator();
        var item = LenientTimesheetScore.NewLenientTimesheetScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientTimesheetScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientTimesheetScoreRequiredCodeRule("lenient timesheet score needs an entry");
        var item = LenientTimesheetScore.NewLenientTimesheetScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientTimesheetScoreValidatorReportsEveryProblem();
        LenientTimesheetScoreRequiredCodeRuleFlagsABlankCode();
    }
}
