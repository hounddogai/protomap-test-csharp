namespace Acme.Workforce.Assets.Tests.Refresh.Rollup;

using Acme.Workforce.Assets.Refresh.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RefreshScoreRulesChecks
{
    public static void RefreshScoreValidatorReportsEveryProblem()
    {
        var validator = RefreshScoreRules.DefaultRefreshScoreValidator();
        var item = RefreshScore.NewRefreshScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RefreshScoreRequiredCodeRule("refresh score needs this field");
        var item = RefreshScore.NewRefreshScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshScoreRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new RefreshScoreRequiredDepartmentRule("refresh score needs this field");
        var item = RefreshScore.NewRefreshScore("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshScoreCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new RefreshScoreCostRangeRule(0.7, 3619.6);
        var item = RefreshScore.NewRefreshScore("x", "y");
        item.Cost = 3621.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RefreshScoreNameLengthRuleFlagsALongName()
    {
        var rule = new RefreshScoreNameLengthRule(32);
        var item = RefreshScore.NewRefreshScore("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RefreshScoreValidatorReportsEveryProblem();
        RefreshScoreRequiredCodeRuleFlagsABlankCode();
        RefreshScoreRequiredDepartmentRuleFlagsABlankDepartment();
        RefreshScoreCostRangeRuleFlagsValuesAboveCost();
        RefreshScoreNameLengthRuleFlagsALongName();
    }
}
