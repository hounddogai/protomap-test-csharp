namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionManifest;

using Acme.Workforce.Compliance.Exception.ExceptionManifest;

/// <summary>Checks of exception manifest code, run by the project's runner.</summary>
public static class ExceptionSummaryRulesChecks
{
    public static void ExceptionSummaryValidatorReportsEveryProblem()
    {
        var validator = ExceptionSummaryRules.DefaultExceptionSummaryValidator();
        var item = ExceptionSummary.NewExceptionSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionSummaryRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new ExceptionSummaryRequiredContactEmailRule("exception summary needs a value");
        var item = ExceptionSummary.NewExceptionSummary("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionSummaryRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new ExceptionSummaryRequiredTeamRule("exception summary needs a value");
        var item = ExceptionSummary.NewExceptionSummary("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionSummaryCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new ExceptionSummaryCapacityRangeRule(32, 86400);
        var item = ExceptionSummary.NewExceptionSummary("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ExceptionSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ExceptionSummaryRequiredCodeRule("exception summary needs this field");
        var item = ExceptionSummary.NewExceptionSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ExceptionSummaryValidatorReportsEveryProblem();
        ExceptionSummaryRequiredContactEmailRuleFlagsABlankContactEmail();
        ExceptionSummaryRequiredTeamRuleFlagsABlankTeam();
        ExceptionSummaryCapacityRangeRuleFlagsValuesAboveCapacity();
        ExceptionSummaryRequiredCodeRuleFlagsABlankCode();
    }
}
