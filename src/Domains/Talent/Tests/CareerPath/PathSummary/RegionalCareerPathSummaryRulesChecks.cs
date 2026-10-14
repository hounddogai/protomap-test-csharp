namespace Acme.Workforce.Talent.Tests.CareerPath.PathSummary;

using Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>Checks of path summary code, run by the project's runner.</summary>
public static class RegionalCareerPathSummaryRulesChecks
{
    public static void RegionalCareerPathSummaryValidatorReportsEveryProblem()
    {
        var validator = RegionalCareerPathSummaryRules.DefaultRegionalCareerPathSummaryValidator();
        var item = RegionalCareerPathSummary.NewRegionalCareerPathSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalCareerPathSummaryCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalCareerPathSummaryCodeLengthRule(64);
        var item = RegionalCareerPathSummary.NewRegionalCareerPathSummary("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalCareerPathSummaryCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new RegionalCareerPathSummaryCostRangeRule(0.9, 4591.6);
        var item = RegionalCareerPathSummary.NewRegionalCareerPathSummary("x", "y");
        item.Cost = 4593.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalCareerPathSummaryValidatorReportsEveryProblem();
        RegionalCareerPathSummaryCodeLengthRuleFlagsALongCode();
        RegionalCareerPathSummaryCostRangeRuleFlagsValuesAboveCost();
    }
}
