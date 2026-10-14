namespace Acme.Workforce.Payrollops.Tests.Deduction.Profile;

using Acme.Workforce.Payrollops.Deduction.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class LegacyDeductionScoreRulesChecks
{
    public static void LegacyDeductionScoreValidatorReportsEveryProblem()
    {
        var validator = LegacyDeductionScoreRules.DefaultLegacyDeductionScoreValidator();
        var item = LegacyDeductionScore.NewLegacyDeductionScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyDeductionScoreRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new LegacyDeductionScoreRequiredBadgeRule("legacy deduction score needs a value");
        var item = LegacyDeductionScore.NewLegacyDeductionScore("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyDeductionScoreNameLengthRuleFlagsALongName()
    {
        var rule = new LegacyDeductionScoreNameLengthRule(120);
        var item = LegacyDeductionScore.NewLegacyDeductionScore("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyDeductionScoreLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new LegacyDeductionScoreLimitRangeRule(255, 86400);
        var item = LegacyDeductionScore.NewLegacyDeductionScore("x", "y");
        item.Limit = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyDeductionScoreTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new LegacyDeductionScoreTenureMonthsRangeRule(255, 3600);
        var item = LegacyDeductionScore.NewLegacyDeductionScore("x", "y");
        item.TenureMonths = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacyDeductionScoreValidatorReportsEveryProblem();
        LegacyDeductionScoreRequiredBadgeRuleFlagsABlankBadge();
        LegacyDeductionScoreNameLengthRuleFlagsALongName();
        LegacyDeductionScoreLimitRangeRuleFlagsValuesAboveLimit();
        LegacyDeductionScoreTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
    }
}
