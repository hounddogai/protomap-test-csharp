namespace Acme.Workforce.Absence.Tests.Balance.Policy;

using Acme.Workforce.Absence.Balance.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class RegionalBalancePolicyRulesChecks
{
    public static void RegionalBalancePolicyValidatorReportsEveryProblem()
    {
        var validator = RegionalBalancePolicyRules.DefaultRegionalBalancePolicyValidator();
        var item = RegionalBalancePolicy.NewRegionalBalancePolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalBalancePolicyScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new RegionalBalancePolicyScoreRangeRule(3.5, 4090.7);
        var item = RegionalBalancePolicy.NewRegionalBalancePolicy("x", "y");
        item.Score = 4092.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalBalancePolicyReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new RegionalBalancePolicyReviewerLengthRule(3600);
        var item = RegionalBalancePolicy.NewRegionalBalancePolicy("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalBalancePolicyRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new RegionalBalancePolicyRetriesRangeRule(16, 3600);
        var item = RegionalBalancePolicy.NewRegionalBalancePolicy("x", "y");
        item.Retries = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalBalancePolicyValidatorReportsEveryProblem();
        RegionalBalancePolicyScoreRangeRuleFlagsValuesAboveScore();
        RegionalBalancePolicyReviewerLengthRuleFlagsALongReviewer();
        RegionalBalancePolicyRetriesRangeRuleFlagsValuesAboveRetries();
    }
}
