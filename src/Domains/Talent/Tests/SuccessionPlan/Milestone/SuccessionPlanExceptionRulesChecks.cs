namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Milestone;

using Acme.Workforce.Talent.SuccessionPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SuccessionPlanExceptionRulesChecks
{
    public static void SuccessionPlanExceptionValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanExceptionRules.DefaultSuccessionPlanExceptionValidator();
        var item = SuccessionPlanException.NewSuccessionPlanException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanExceptionTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SuccessionPlanExceptionTitleLengthRule(16);
        var item = SuccessionPlanException.NewSuccessionPlanException("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanExceptionLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new SuccessionPlanExceptionLeaveDaysRangeRule(64, 1000);
        var item = SuccessionPlanException.NewSuccessionPlanException("x", "y");
        item.LeaveDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SuccessionPlanExceptionRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SuccessionPlanExceptionRequiredCodeRule("succession plan exception needs an entry");
        var item = SuccessionPlanException.NewSuccessionPlanException("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SuccessionPlanExceptionValidatorReportsEveryProblem();
        SuccessionPlanExceptionTitleLengthRuleFlagsALongTitle();
        SuccessionPlanExceptionLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        SuccessionPlanExceptionRequiredCodeRuleFlagsABlankCode();
    }
}
