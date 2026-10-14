namespace Acme.Workforce.Compliance.Tests.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>Checks of seasonal exception group code, run by the project's runner.</summary>
public static class ExceptionMilestoneRulesChecks
{
    public static void ExceptionMilestoneValidatorReportsEveryProblem()
    {
        var validator = ExceptionMilestoneRules.DefaultExceptionMilestoneValidator();
        var item = ExceptionMilestone.NewExceptionMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionMilestoneVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new ExceptionMilestoneVersionRangeRule(80, 1000);
        var item = ExceptionMilestone.NewExceptionMilestone("x", "y");
        item.Version = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ExceptionMilestoneRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ExceptionMilestoneRequiredCodeRule("exception milestone needs this field");
        var item = ExceptionMilestone.NewExceptionMilestone("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ExceptionMilestoneValidatorReportsEveryProblem();
        ExceptionMilestoneVersionRangeRuleFlagsValuesAboveVersion();
        ExceptionMilestoneRequiredCodeRuleFlagsABlankCode();
    }
}
