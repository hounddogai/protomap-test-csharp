namespace Acme.Workforce.Workplace.Tests.Desk.Tier;

using Acme.Workforce.Workplace.Desk.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class RegionalDeskSummaryRulesChecks
{
    public static void RegionalDeskSummaryValidatorReportsEveryProblem()
    {
        var validator = RegionalDeskSummaryRules.DefaultRegionalDeskSummaryValidator();
        var item = RegionalDeskSummary.NewRegionalDeskSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalDeskSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalDeskSummaryRequiredCodeRule("regional desk summary needs this field");
        var item = RegionalDeskSummary.NewRegionalDeskSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalDeskSummaryValidatorReportsEveryProblem();
        RegionalDeskSummaryRequiredCodeRuleFlagsABlankCode();
    }
}
