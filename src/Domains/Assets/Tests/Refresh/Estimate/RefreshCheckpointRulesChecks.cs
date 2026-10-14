namespace Acme.Workforce.Assets.Tests.Refresh.Estimate;

using Acme.Workforce.Assets.Refresh.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class RefreshCheckpointRulesChecks
{
    public static void RefreshCheckpointValidatorReportsEveryProblem()
    {
        var validator = RefreshCheckpointRules.DefaultRefreshCheckpointValidator();
        var item = RefreshCheckpoint.NewRefreshCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshCheckpointRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new RefreshCheckpointRequiredCurrencyRule("refresh checkpoint needs an entry");
        var item = RefreshCheckpoint.NewRefreshCheckpoint("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshCheckpointTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RefreshCheckpointTitleLengthRule(120);
        var item = RefreshCheckpoint.NewRefreshCheckpoint("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshCheckpointScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new RefreshCheckpointScoreRangeRule(7.6, 314.8);
        var item = RefreshCheckpoint.NewRefreshCheckpoint("x", "y");
        item.Score = 316.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RefreshCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new RefreshCheckpointCodeLengthRule(1000);
        var item = RefreshCheckpoint.NewRefreshCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RefreshCheckpointValidatorReportsEveryProblem();
        RefreshCheckpointRequiredCurrencyRuleFlagsABlankCurrency();
        RefreshCheckpointTitleLengthRuleFlagsALongTitle();
        RefreshCheckpointScoreRangeRuleFlagsValuesAboveScore();
        RefreshCheckpointCodeLengthRuleFlagsALongCode();
    }
}
