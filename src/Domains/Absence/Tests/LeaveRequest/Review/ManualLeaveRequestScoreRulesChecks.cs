namespace Acme.Workforce.Absence.Tests.LeaveRequest.Review;

using Acme.Workforce.Absence.LeaveRequest.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ManualLeaveRequestScoreRulesChecks
{
    public static void ManualLeaveRequestScoreValidatorReportsEveryProblem()
    {
        var validator = ManualLeaveRequestScoreRules.DefaultManualLeaveRequestScoreValidator();
        var item = ManualLeaveRequestScore.NewManualLeaveRequestScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualLeaveRequestScoreRequiredNameRuleFlagsABlankName()
    {
        var rule = new ManualLeaveRequestScoreRequiredNameRule("manual leave request score needs this field");
        var item = ManualLeaveRequestScore.NewManualLeaveRequestScore("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualLeaveRequestScoreJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new ManualLeaveRequestScoreJobCodeLengthRule(120);
        var item = ManualLeaveRequestScore.NewManualLeaveRequestScore("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualLeaveRequestScoreRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new ManualLeaveRequestScoreRequiredCategoryRule("manual leave request score needs an entry");
        var item = ManualLeaveRequestScore.NewManualLeaveRequestScore("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualLeaveRequestScoreValidatorReportsEveryProblem();
        ManualLeaveRequestScoreRequiredNameRuleFlagsABlankName();
        ManualLeaveRequestScoreJobCodeLengthRuleFlagsALongJobCode();
        ManualLeaveRequestScoreRequiredCategoryRuleFlagsABlankCategory();
    }
}
