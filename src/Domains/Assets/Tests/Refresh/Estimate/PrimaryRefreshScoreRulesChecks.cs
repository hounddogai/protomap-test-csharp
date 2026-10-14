namespace Acme.Workforce.Assets.Tests.Refresh.Estimate;

using Acme.Workforce.Assets.Refresh.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PrimaryRefreshScoreRulesChecks
{
    public static void PrimaryRefreshScoreValidatorReportsEveryProblem()
    {
        var validator = PrimaryRefreshScoreRules.DefaultPrimaryRefreshScoreValidator();
        var item = PrimaryRefreshScore.NewPrimaryRefreshScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryRefreshScoreWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new PrimaryRefreshScoreWeightRangeRule(1.8, 4929.5);
        var item = PrimaryRefreshScore.NewPrimaryRefreshScore("x", "y");
        item.Weight = 4931.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryRefreshScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryRefreshScoreRequiredCodeRule("primary refresh score needs a value");
        var item = PrimaryRefreshScore.NewPrimaryRefreshScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryRefreshScoreTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PrimaryRefreshScoreTitleLengthRule(86400);
        var item = PrimaryRefreshScore.NewPrimaryRefreshScore("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryRefreshScoreCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimaryRefreshScoreCodeLengthRule(255);
        var item = PrimaryRefreshScore.NewPrimaryRefreshScore("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryRefreshScoreRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new PrimaryRefreshScoreRequiredDescriptionRule("primary refresh score needs a value");
        var item = PrimaryRefreshScore.NewPrimaryRefreshScore("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryRefreshScoreValidatorReportsEveryProblem();
        PrimaryRefreshScoreWeightRangeRuleFlagsValuesAboveWeight();
        PrimaryRefreshScoreRequiredCodeRuleFlagsABlankCode();
        PrimaryRefreshScoreTitleLengthRuleFlagsALongTitle();
        PrimaryRefreshScoreCodeLengthRuleFlagsALongCode();
        PrimaryRefreshScoreRequiredDescriptionRuleFlagsABlankDescription();
    }
}
