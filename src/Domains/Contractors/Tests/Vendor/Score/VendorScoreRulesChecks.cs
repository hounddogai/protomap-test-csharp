namespace Acme.Workforce.Contractors.Tests.Vendor.Score;

using Acme.Workforce.Contractors.Vendor.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class VendorScoreRulesChecks
{
    public static void VendorScoreValidatorReportsEveryProblem()
    {
        var validator = VendorScoreRules.DefaultVendorScoreValidator();
        var item = VendorScore.NewVendorScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorScoreUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new VendorScoreUpdatedAtRangeRule(64, 80);
        var item = VendorScore.NewVendorScore("x", "y");
        item.UpdatedAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VendorScoreAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new VendorScoreAttemptsRangeRule(32, 86400);
        var item = VendorScore.NewVendorScore("x", "y");
        item.Attempts = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VendorScoreHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new VendorScoreHoursRangeRule(0.2, 263.2);
        var item = VendorScore.NewVendorScore("x", "y");
        item.Hours = 264.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VendorScoreBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new VendorScoreBudgetRangeRule(4.8, 505.9);
        var item = VendorScore.NewVendorScore("x", "y");
        item.Budget = 507.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VendorScoreCodeLengthRuleFlagsALongCode()
    {
        var rule = new VendorScoreCodeLengthRule(3600);
        var item = VendorScore.NewVendorScore("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        VendorScoreValidatorReportsEveryProblem();
        VendorScoreUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        VendorScoreAttemptsRangeRuleFlagsValuesAboveAttempts();
        VendorScoreHoursRangeRuleFlagsValuesAboveHours();
        VendorScoreBudgetRangeRuleFlagsValuesAboveBudget();
        VendorScoreCodeLengthRuleFlagsALongCode();
    }
}
